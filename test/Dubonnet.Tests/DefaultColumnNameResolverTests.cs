using System.ComponentModel.DataAnnotations.Schema;
using Dubonnet.Abstractions;
using Dubonnet.Resolvers;
using Dubonnet;
using Xunit;

namespace Dubonnet.Tests
{
    public class DefaultColumnNameResolverTests
    {
        private static readonly DefaultColumnNameResolver Resolver = new DefaultColumnNameResolver();

        [Fact]
        public void ResolvesName()
        {
            var type = typeof(Foo);
            var name = Resolver.Resolve(new DubonProperty(type,type.GetProperty("Bar")));
            Assert.Equal("Bar", name);
        }

        [Fact]
        public void ResolvesColumnAttribute()
        {
            var type = typeof(Bar);
            var name = Resolver.Resolve(new DubonProperty(type,type.GetProperty("FooBarBaz")));
            Assert.Equal("foo_bar_baz", name);
        }

        private class Foo
        {
            public string Bar { get; set; }
        }

        private class Bar
        {
            [Column("foo_bar_baz")]
            public string FooBarBaz { get; set; }
        }
    }
}
