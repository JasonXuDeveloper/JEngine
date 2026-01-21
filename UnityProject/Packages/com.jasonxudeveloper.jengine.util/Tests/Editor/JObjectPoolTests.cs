// JObjectPoolTests.cs
// Unit tests for JObjectPool<T>

using System.Collections.Generic;
using NUnit.Framework;

namespace JEngine.Util.Tests
{
    [TestFixture]
    public class JObjectPoolTests
    {
        private class TestObject
        {
            public int Value;
            public bool WasReset;
        }

        [Test]
        public void Rent_FromEmptyPool_CreatesNewInstance()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 4);

            var obj = pool.Rent();

            Assert.IsNotNull(obj);
            Assert.AreEqual(0, obj.Value);
        }

        [Test]
        public void Return_ThenRent_ReusesSameInstance()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 4);

            var obj1 = pool.Rent();
            obj1.Value = 42;
            pool.Return(obj1);

            var obj2 = pool.Rent();

            Assert.AreSame(obj1, obj2);
            Assert.AreEqual(42, obj2.Value); // Value preserved (no onReturn callback)
        }

        [Test]
        public void Return_WithOnReturnCallback_InvokesCallback()
        {
            var pool = new JObjectPool<TestObject>(
                maxSize: 4,
                onReturn: static obj => obj.WasReset = true
            );

            var obj = pool.Rent();
            obj.WasReset = false;
            pool.Return(obj);

            var reused = pool.Rent();

            Assert.IsTrue(reused.WasReset);
        }

        [Test]
        public void Rent_WithOnRentCallback_InvokesCallback()
        {
            var pool = new JObjectPool<TestObject>(
                maxSize: 4,
                onRent: static obj => obj.Value = 99
            );

            var obj = pool.Rent();

            Assert.AreEqual(99, obj.Value);
        }

        [Test]
        public void Return_ExceedsMaxSize_DiscardsExcess()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 2);

            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            var obj3 = pool.Rent();

            pool.Return(obj1);
            pool.Return(obj2);
            pool.Return(obj3); // This should be discarded

            Assert.AreEqual(2, pool.Count);
        }

        [Test]
        public void Return_NullObject_DoesNotThrow()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 4);

            Assert.DoesNotThrow(() => pool.Return(null));
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Clear_RemovesAllPooledObjects()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 4);

            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            pool.Return(obj1);
            pool.Return(obj2);
            Assert.AreEqual(2, pool.Count);

            pool.Clear();

            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Prewarm_CreatesSpecifiedNumberOfObjects()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 10);

            pool.Prewarm(5);

            Assert.AreEqual(5, pool.Count);
        }

        [Test]
        public void Prewarm_DoesNotExceedMaxSize()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 3);

            pool.Prewarm(10);

            Assert.AreEqual(3, pool.Count);
        }

        [Test]
        public void SharedPool_ReturnsSameInstanceForType()
        {
            var pool1 = JObjectPool.Shared<TestObject>();
            var pool2 = JObjectPool.Shared<TestObject>();

            Assert.AreSame(pool1, pool2);
        }

        [Test]
        public void SharedPool_DifferentTypesHaveDifferentPools()
        {
            var pool1 = JObjectPool.Shared<TestObject>();
            var pool2 = JObjectPool.Shared<List<int>>();

            Assert.AreNotSame(pool1, pool2);
        }

        [Test]
        public void Count_ReflectsPooledObjectCount()
        {
            var pool = new JObjectPool<TestObject>(maxSize: 10);

            Assert.AreEqual(0, pool.Count);

            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            Assert.AreEqual(0, pool.Count); // Objects are rented, not in pool

            pool.Return(obj1);
            Assert.AreEqual(1, pool.Count);

            pool.Return(obj2);
            Assert.AreEqual(2, pool.Count);

            pool.Rent();
            Assert.AreEqual(1, pool.Count);
        }
    }
}
