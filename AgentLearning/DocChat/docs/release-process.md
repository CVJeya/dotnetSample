# Release Process (sample, fictional)

Production releases happen every second Tuesday. Code freeze starts at 5 PM on the Monday before the release.

Every release needs two approvals: one from a developer who did not write the change, and one from the product owner.

After release, watch the error rate for 15 minutes. If it goes above 2 percent, roll back immediately and open an incident.

The staging server is redeployed from the main branch every Friday night, so do not leave long-running tests on staging over the weekend.
