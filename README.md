# Backwords

An etymology explorer: search a word, see its lineage traced back through history, and click into related words to go down a rabbit hole.

## What this is

Backwords lets you look up a word (starting with a curated set of
software/computing terms) and see how it evolved — which languages it passed through, what it was borrowed or inherited from, and how it connects to other words. Related words are clickable, so you can follow the trail as far as it
goes.

## Stack

- **Backend:** C# / .NET Web API
- **Database:** SQLite
- **Frontend:** React (Vite) + React Router

See [DESIGN.md](DESIGN.md) for the full architecture, schema, and API design.

## Data Source & Attribution

The etymology data underlying this project is sourced from
**[etymology-db](https://github.com/droher/etymology-db)** by droher, a structured dataset of etymological relationships parsed from **[Wiktionary](https://en.wiktionary.org)**, the free multilingual dictionary
project of the Wikimedia Foundation.

Huge thanks to both droher and Wiktionary, because this passion project wouldn't be possible without their contributions.

- Wiktionary content is licensed **CC BY-SA 4.0** / GFDL.
- etymology-db's data is licensed **CC BY-SA 3.0**; its extraction code is licensed **Apache 2.0**.
- Backwords imports and re-normalizes a curated subset of this data into its own SQLite schema (see [DESIGN.md](DESIGN.md)) — full credit for the underlying research and extraction work belongs to droher and the Wiktionary contributor
  community, not to this project.

If you reuse or redistribute the data (not just the code) from this project, it should carry the same CC BY-SA attribution and share-alike terms.

## Status

Early development. Not yet runnable end-to-end.

## License

MIT (code). See [Data Source & Attribution](#data-source--attribution) above for the separate license terms covering the underlying etymology data.
