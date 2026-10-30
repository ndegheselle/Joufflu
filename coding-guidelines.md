# Coding guidelines
Goal: minimize the mental overhead needed to read, change and trust the code.

> When a rule hurts readability in a specific case, readability wins. Explain the exception in a comment.

## Structure
- **Feature first**: group code by feature, not by technical layer. A feature's UI, logic and data access live together.
- **Shallow call paths**: from a feature's entry point, the actual implementation should be 2-3 hops away maximum.
- **Locality**: code that changes together lives together.

## Code
- **Separate logic and UI**: the UI should only display state and forward user actions to functions.
- **Flat control flow**: prefer early returns over nested `if`/loops.
- **No nested calls**: name intermediate results instead of `a(b(c(x)))`.
- **Self-explanatory names**: if a name needs a comment, rename it.
- **Explicit over implicit**: no hidden side effects, no clever tricks.
- **Comments explain why**, not what.

## Scope
- **Every feature has a cost**: code to maintain, test and understand. Before adding or changing one, check that it serves the project's goals.
- **Removing is progress**: delete unused code and features.
- **Challenge decisions**: question whether a new functionality is relevant to the project.

## Tests
Goal: catch what would hurt, at the lowest maintenance cost. Other coding rules also apply for tests.

*What to test* :
- **Critical paths**: Business rules and anything hard to spot in production.
- **Skip the trivial**: getters, mappings, framework glue, plain UI layout.
- **Bug fixes**: add a test, reproduce the bug, then fix it.

*How to write a test* :
- **Test behavior, not implementation**
- **One behavior per test**: named after it (`rejects_order_when_stock_is_empty`).
- **Mock only the edges**: fake what's slow or external (network, payment).
- **Fast and deterministic**: no reliance on time, order, or shared state.

*When to delete a test* :
- **It's unstable**: if it breaks often without catching real bugs, fix it or delete it.
- **It's unused**: it guards code or behavior that no longer matters.

*Testing the UI* :
- **Logic outside the UI**: tested with fast unit tests, with no rendering involved (cf. "*Separate logic and UI*").
- **Component tests**: render the component, interact like a user would (click, type, select), and check what the user sees. Don't check internal state or implementation details.
- **Visual regression tests (optional)**: Review every diff before accepting it.

## Documentation
Goal: give readers the context the code can't, at the lowest maintenance cost. Other rules also apply to docs.

*What to document* :
- **The why, not the what**: decisions, trade-offs, constraints. The code already says what it does.
- **Entry points**: how to run, build, test and deploy. A newcomer should start in minutes.
- **Non-obvious behavior**: business rules, edge cases, known pitfalls.

*How to write a doc* :
- **Answer first**: lead with the conclusion or the command, details after.
- **One topic per doc**: named after it (`how-to-deploy.md`, not `notes.md`).
- **Short sentences, plain words**: if a sentence needs rereading, split it.
- **Show, don't describe**: a working example beats a paragraph.

*Where docs live* :
- **Feature first**: organize docs by feature, not by type (cf. "*Feature first*").
- **One source of truth**: link instead of duplicating.

*When to update or delete a doc* :
- **Code changes, doc changes**: update the doc in the same commit as the behavior it describes.
- **It's wrong or obsolete**: a doc that misleads is worse than no doc. Fix it or delete it.