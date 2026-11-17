// Each integration-test class starts its own SQL LocalDB and application host.
// Keep class-level resource use bounded; concurrent purchase/check-in tests still run their own parallel requests.
[assembly: Xunit.CollectionBehavior(MaxParallelThreads = 2)]
