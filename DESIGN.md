# Backwords — Design Notes

An etymology explorer: search a word, see its lineage traced back through history,
and click into related words to go down a rabbit hole.

## Stack

- **Backend:** C# / .NET Web API
- **Database:** SQLite (chosen deliberately over SQL Server for zero-setup portability —
  anyone cloning the repo can run it immediately, no external server required)
- **Frontend:** React (Vite) + React Router
- **Data source:** [droher/etymology-db](https://github.com/droher/etymology-db) — a
  structured dataset of ~3.8M etymological relationships parsed from Wiktionary.
  - Data license: CC BY-SA 3.0 (attribution + share-alike required)
  - Extraction code license: Apache 2.0
  - Underlying source: Wiktionary content is CC BY-SA 4.0 / GFDL
  - **Action item:** credit droher/etymology-db and Wiktionary in the repo README;
    if the derived SQLite dataset is published as part of this repo, it should carry
    the same CC BY-SA notice.

## Scope for v1

- A curated seed set of English headwords (~100–300), not the full 3.8M-row dataset.
- All 31 relationship types from etymology-db are preserved from the start (no
  point gating this — it's just a string column, easier to keep them all now than
  to backfill later).
- Lineage depth is arbitrarily deep (recursive), not fixed — same reasoning: a
  recursive query isn't harder to write than a fixed-depth one, so there's no
  reason to constrain it artificially.
- **Not in v1 (explicit wish-list):** etymology of idioms/phrases, not just single
  words. A real, deliberate future direction — noted here so it doesn't get lost,
  not something to scope-creep into v1.

## Data Model

Two tables, deliberately normalized (lexemes stored once, referenced by id) rather
than mirroring etymology-db's denormalized one-row-per-relationship CSV shape.

**Correction (post-data-inspection):** etymology-db's `lang` column holds full
language names ("English", "Middle English", "Proto-Indo-European"), not short
codes. The schema below reflects this — no code-mapping table needed.

```
Table: lexemes
  id            (primary key)
  term          (the word/term text, e.g. "etymology", "ethimologie", "etymologia")
  language      (full name, e.g. "English", "Old French", "Latin", "Ancient Greek" —
                 matches etymology-db's own `lang` column directly)
  is_seed_word  (bool — was this one of the curated starting English words, vs. an
                 ancestor pulled in to complete a chain?)

Table: derivations
  id                (primary key)
  term_id           (FK -> lexemes.id)           -- the word being derived/explained
  related_term_id   (FK -> lexemes.id, nullable) -- what it came from (null for root/unattached nodes)
  relation_type     (one of etymology-db's 31 types: inherited_from, borrowed_from,
                     derived_from, compound_of, cognate_of, doublet_with, ... )
  position          (nullable int — ordering for compounds/affixes, mirrors source data)
```

### Relationship types: two categories

- **Chain relations** (walked recursively to build the vertical lineage spine):
  `inherited_from`, `borrowed_from`, `derived_from`, `compound_of`, `root`,
  and similar direct-derivation types.
- **Sideways relations** (looked up per-node, shown as "see related" callouts,
  NOT walked recursively — these connect words without one being an ancestor
  of the other): `cognate_of`, `doublet_with`, `etymologically_related_to`,
  and similar.

This split exists because the UI itself is two different shapes: a straight-line
vertical chain (ancestor to ancestor) with optional side-branches per node
(related-but-not-ancestral words).

### Real-data complication: multiple attested etymology paths

Inspecting real etymology-db rows (e.g. "algorithm") showed the source data is
not always a clean single-parent chain. Wiktionary sometimes records multiple
attested derivation paths for one word (grouped via `group_tag`/`parent_tag`/
`position` in the source CSV), and a `group_derived_root` cluster's rows are
siblings anchored to the *same* term rather than a recursive word-to-word
chain — the true recursive jump (looking up a related term's own rows to
continue further back) only happens when you follow `related_term_id` into
that term's own term+language pair.

**v1 policy (a deliberate, documented simplification):** for each word, only
chain-type relations are considered for the primary lineage spine
(`inherited_from`, `borrowed_from`, `derived_from`, `root`, and similar).
When a word has multiple candidate groups/positions, **position 0 of the
first relevant group is taken as the primary next hop**, and the walk
recurses into that related term's own rows from there. Every other row for
that word (alternate positions, alternate groups, and all sideways relation
types) is preserved as "related" data on that node, not discarded — it's
just not part of the primary spine.

This means Backwords shows *one* primary etymology path per word when
multiple are attested by Wiktionary, with alternates surfaced as related
terms rather than additional spine branches. This is an intentional, stated
scope decision, not a silent gap — worth saying explicitly in the UI/README
if this ever comes up (e.g. a small "showing the primary attested path;
other theories exist" note).

### Data quality notes from real inspection

- Some `related_term` values are blank/whitespace-only in the source data
  (e.g. an "Ancient Greek" root for "panentheism"). Import logic must trim
  and treat blank as null/skip, not as a real term.
- etymology-db's own `term_id` is a deterministic hash of (term, lang) — useful
  as an in-memory dedup key *during import only*. It is NOT the same as this
  project's own `Lexeme.Id` (an autoincrement int assigned by SQLite/EF Core).
  The import script's job is to map etymology-db's hash → this project's own
  int id via a lookup dictionary; the source hash itself does not need to be
  persisted in the database.

## Recursive Lineage Query

SQLite `WITH RECURSIVE` CTE, walking `derivations.related_term_id` chains
starting from a seed word until no further links exist.

```sql
WITH RECURSIVE lineage AS (
    -- Anchor: the word we start from
    SELECT id, term, language_code, related_term_id, relation_type, 0 AS depth
    FROM lexemes JOIN derivations ON lexemes.id = derivations.term_id
    WHERE lexemes.term = :seed_word AND lexemes.language_code = :seed_lang

    UNION ALL

    -- Recursive step: for each row so far, find what ITS related_term derives from
    SELECT l.id, l.term, l.language_code, d.related_term_id, d.relation_type, lineage.depth + 1
    FROM lineage
    JOIN lexemes l ON l.id = lineage.related_term_id
    JOIN derivations d ON d.term_id = l.id
    WHERE lineage.depth < :safety_cap   -- guardrail against data cycles, e.g. 50
)
SELECT * FROM lineage;
```

Note: `depth < :safety_cap` is a defensive guardrail against cycles in the source
data (etymology has real loops — doublets, reborrowings), not a business-logic
limit on lineage depth. These are two different concerns and shouldn't be conflated.

Sideways relations are a separate, non-recursive query per spine node:

```sql
SELECT related_term, related_lang, relation_type
FROM derivations
WHERE term_id = :node_id
  AND relation_type IN ('cognate_of', 'doublet_with', 'etymologically_related_to', ...)
```

## API Surface (v1)

```
GET /api/lexemes/search?q=...          -> autocomplete suggestions (matches against
                                           is_seed_word = true entries only, so
                                           autocomplete only surfaces words with
                                           full chains, not every ancestor node)

GET /api/words/{term}/{lang}/lineage   -> full spine + inline related-word data
                                           for one word
```

Response shape for the lineage endpoint (nested JSON, one blob per lookup):

```json
{
  "seed_word": "etymology",
  "summary": "comes from Old French, Latin, and Ancient Greek",
  "chain": [
    {
      "term": "etymology",
      "lang": "English",
      "related": []
    },
    {
      "term": "ethimologie",
      "lang": "Old French",
      "related": [
        { "term": "etimologie", "lang": "Middle French", "in_collection": false }
      ]
    },
    {
      "term": "etymologia",
      "lang": "Latin",
      "related": []
    },
    {
      "term": "ἐτυμολογία",
      "lang": "Ancient Greek",
      "related": [
        { "term": "ἔτυμον", "lang": "Ancient Greek", "in_collection": true }
      ]
    }
  ]
}
```

Every related-word entry carries an `in_collection` boolean, computed server-side,
so the frontend never attempts a lookup for a word that isn't in the dataset —
no dead clicks, no "not found" UX for related terms that were never part of the
curated set.

The `summary` line is derived, not stored: computed by walking the chain and
pulling distinct `lang` values in order (decide: dedupe repeated languages, or
show every hop even with repeats — leaning dedupe for readability).

## Frontend Behavior

- **Routing:** React Router, word-in-URL (e.g. `/word/etymology/en`) so browser
  back/forward works natively and every lookup is a shareable link.
- **Rabbit-holing:** related words flagged `in_collection: true` are clickable and
  navigate to their own lineage view. Words flagged `false` are inert display text.
- **Cycles:** if a rabbit hole leads back to an already-visited word, it's just a
  fresh page load — no cycle-detection/shortcut logic in v1.
- **Search:** plain text box with autocomplete, backed by `/api/lexemes/search`.

## Planned Enhancements (not in v1)

- **Idiom etymology** (already noted above): tracing the origins of idioms/phrases,
  not just single words.
- **Word-sense definitions per lexeme.** etymology-db only carries etymological
  *relationships* — no definitions/glosses, confirmed by inspecting its own
  columns and real rows (e.g. "cyrnel" (Old English) has only structural
  relations, no meaning data). Without definitions, Backwords can show a
  word's form changing across history but not its *meaning* changing (e.g.
  "kernel" narrowing from a general seed/core sense, or the well-documented
  "navel"/"hub" semantic drift from "central point" to "belly button"
  specifically) — a real gap, since sense-shift is one of the more
  interesting parts of etymology.
  - Wiktionary itself (the same underlying source as etymology-db) does carry
    definitions, via the MediaWiki Action API (`action=query&prop=...`) or
    the newer REST API (`/api/rest_v1/page/definition/{term}`) — same CC
    BY-SA 4.0 license already covering this project, no API key required.
  - Real complications to plan for before building this: Wiktionary pages
    are keyed by modern spelling and can hold multiple languages/senses per
    page, so older/reconstructed ancestor forms (e.g. Proto-Germanic,
    Proto-Indo-European roots) may have thin or no coverage. This would be a
    second fetch-and-parse import pass (likely one HTTP call per unique
    lexeme), needing rate-limiting and caching rather than live per-request
    lookups, writing into a new `Definition` field on `Lexeme` (or a related
    table if multiple senses need to be preserved).
  - **Sequencing decision:** build this as a v1.5 enrichment pass *after* the
    core spine/API/frontend loop works end to end — not woven into the
    current import pipeline while it's still being built.

## Attribution

This project's dataset derives from [etymology-db](https://github.com/droher/etymology-db)
by droher, which parses [Wiktionary](https://en.wiktionary.org) content.
Wiktionary content is licensed CC BY-SA 4.0 / GFDL. etymology-db's data is licensed
CC BY-SA 3.0; its extraction code is Apache 2.0.
