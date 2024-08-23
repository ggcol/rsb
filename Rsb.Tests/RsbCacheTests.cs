using Moq;
using Rsb.Core;
using Rsb.Core.Caching;

namespace Rsb.Tests;

[TestFixture]
public class RsbCacheTests
{
    private IRsbCache _cache;
    
    [SetUp]
    public void SetUp()
    {
        _cache = new RsbCache();
    }
    
    [Test]
    public void Set_GivenAKeyAndAnObject_StoresInCache()
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        _cache.Set(key, obj);
        
        //Act
        var exists = _cache.TryGetValue(key, out _);

        //Assert
        Assert.That(exists, Is.True);
    }
    
    [Test]
    public void TryGetValue_GivenAKey_RetrieveRelativeCachedObject()
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        _cache.Set(key, obj);
        
        //Act
        _ = _cache.TryGetValue(key, out var retrieved);

        //Assert
        Assert.That(retrieved, Is.EqualTo(obj));
    }

    [TestCase(5, 1, true)]
    [TestCase(2, 20, false)]
    public void TryGetValue_GivenExpiration_ReturnsAccordingToExpiration(
        int expiresAfterMilliseconds, int waitingMilliseconds, bool expected)
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        var expiresAfter = TimeSpan.FromMilliseconds(expiresAfterMilliseconds);
        _cache.Set(key, obj, expiresAfter);
        
        //Act
        Thread.Sleep(waitingMilliseconds);
        var result = _cache.TryGetValue(key, out _);
        
        Assert.That(result, Is.EqualTo(expected));
    }
    
    
}