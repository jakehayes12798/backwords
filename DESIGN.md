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

```
Table: lexemes
  id            (primary key)
  term          (the word/term text, e.g. "etymology", "ethimologie", "etymologia")
  language_code (e.g. "en", "fro" for Old French, "la" for Latin, "grc" for Ancient Greek)
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

## Attribution

This project's dataset derives from [etymology-db](https://github.com/droher/etymology-db)
by droher, which parses [Wiktionary](https://en.wiktionary.org) content.
Wiktionary content is licensed CC BY-SA 4.0 / GFDL. etymology-db's data is licensed
CC BY-SA 3.0; its extraction code is Apache 2.0.
