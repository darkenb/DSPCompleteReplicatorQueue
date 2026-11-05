# Changelog

## 1.0.5

- Added a configurable hotkey (default Shift+Y) that completes the queue while the Replicator window is closed.
- Show a message when the queue is empty.

## 1.0.4

- Show an in-game message when completion stops early (missing inputs, changed queue). It used to appear only in the log.
- Added a test that runs the game's own forge code to guard against the intermediate item duplication fixed in 1.0.3.

## 1.0.3

- Use native production instead of granting products directly and clearing the queue.
- Preserve intermediate items, delivery/overflow handling, statistics and events.
- Avoid duplicate overflow transfers and repeated activation.
- Stop safely on changed queues, missing inputs or invalid tasks.
- Limit accelerated work per frame and improve button lifecycle cleanup.

## 1.0.0

- Initial release.
