// Helper to verify expected results
void Check(string test, int expected, int actual)
{
    Console.WriteLine(
        expected == actual
            ? $"PASS: {test}"
            : $"FAIL: {test} | Expected {expected}, Got {actual}"
    );
}

// TEST 1: LRU Eviction
Console.WriteLine("TEST 1: LRU Eviction");

var cache1 = new LRUCache(2);

cache1.Put(1, 100);
cache1.Put(2, 200);
cache1.Get(1);
cache1.Put(3, 300);

Check("Key 2 evicted", -1, cache1.Get(2));
Check("Key 1 remains", 100, cache1.Get(1));
Check("Key 3 exists", 300, cache1.Get(3));


// TEST 2: Update Existing Key
Console.WriteLine("\nTEST 2: Update Existing Key");

var cache2 = new LRUCache(2);

cache2.Put(1, 100);
cache2.Put(2, 200);
cache2.Put(1, 500);
cache2.Put(3, 300);

Check("Updated value", 500, cache2.Get(1));
Check("Key 2 evicted", -1, cache2.Get(2));
Check("New key exists", 300, cache2.Get(3));


// TEST 3: Capacity of One
Console.WriteLine("\nTEST 3: Capacity of One");

var cache3 = new LRUCache(1);

cache3.Put(1, 100);
cache3.Put(2, 200);

Check("Old key removed", -1, cache3.Get(1));
Check("New key exists", 200, cache3.Get(2));


// TEST 4: Missing Key
Console.WriteLine("\nTEST 4: Missing Key");

var cache4 = new LRUCache(2);

cache4.Put(1, 100);
cache4.Put(2, 200);

Check("Missing key", -1, cache4.Get(99));

cache4.Put(3, 300);

Check("Key 1 evicted", -1, cache4.Get(1));
Check("Key 2 remains", 200, cache4.Get(2));