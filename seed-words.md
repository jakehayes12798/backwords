# Backwords Seed Word List (Draft v1)

Curated software/computing vocabulary for the initial dataset. ~130 words,
organized loosely by theme. Each will need to be checked against
etymology-db's coverage during import — some may have thin or missing
etymology chains and need to be dropped or swapped.

## Greatest hits (famously good/surprising stories — confirmed via research)

- robot        (Czech "robota" = forced labor; coined for Čapek's play R.U.R.)
- algorithm    (from al-Khwarizmi, 9th-century Persian/Uzbek mathematician)
- computer     (originally meant a PERSON who computes, not a machine)
- bug          (predates computing by decades — Edison used it in 1878;
                the Hopper moth story is a documented myth, worth debunking
                in the UI as a feature, not just a data point)
- virus        (Latin for "poison"/"slimy liquid," long before computing)
- spam         (from the Monty Python sketch, itself from the canned meat brand)
- cursor       (Latin "currere," to run)
- avatar       (Sanskrit "avatāra," descent of a deity to earth)
- cloud        (as in "the cloud" — worth tracing the plain Old English "clud"
                origin, contrast with the modern computing sense)
- byte         (deliberately coined, respelling of "bite" to avoid confusion
                with "bit" — a fun *engineered* etymology rather than organic)

## Core computing/software vocabulary

- software, hardware, program, compile, debug, execute, kernel, cache,
  buffer, protocol, syntax, semantics, compiler, interpreter, runtime,
  process, thread, stack, queue, heap, pointer, array, string, integer,
  boolean, variable, function, method, class, object, interface, module,
  library, framework, repository, commit, branch, merge, deploy, server,
  client, database, query, index, schema, network, packet, socket,
  firewall, encryption, key, token, session, cookie, cache, latency,
  bandwidth, router, gateway, node, cluster, container, pipeline,
  automation, script, terminal, shell, console, log, debugger, breakpoint,
  exception, error, warning, patch, update, version, release, build,
  test, mock, stub, refactor, legacy, deprecated, license, license,
  open-source, license

## Broader tech/digital vocabulary

- internet, web, browser, hyperlink, domain, url, email, download, upload,
  stream, format, encode, decode, compress, archive, backup, restore,
  sync, cloud, digital, analog, binary, hexadecimal, byte, bit, pixel,
  resolution, render, graphic, interface, widget, dashboard, notification,
  cursor, keyboard, mouse, monitor, printer, scanner, modem, router,
  bandwidth, firmware, driver, plugin, extension, add-on, sandbox,
  emulator, simulator, virtual, avatar, persona, username, password,
  login, logout, session, cookie, spam, phishing, malware, virus, worm,
  trojan, firewall, antivirus, patch, exploit, vulnerability, breach

## Notes

- Several words appear in more than one section above (cache, cloud, byte,
  cookie, spam, virus) — that's fine, just dedupe when building the actual
  seed list file.
- "license"/"open-source" line has an accidental triplicate — dedupe.
- Not every word here is guaranteed to have a rich etymology-db entry (some
  compounds like "open-source" or "add-on" may be too modern/compound to
  trace meaningfully) — treat this as a candidate list to prune during
  actual import, not a final committed list.
- Target: prune/dedupe down to a clean ~100-130 word final list once you
  see which ones actually resolve to real chains in etymology-db.
