namespace Container.Tests {

public class Tests
{
	interface IFoo {}
	class Foo : IFoo {};
    class OtherFoo : IFoo {};

    [Fact]
    public void Test1()     //singleton poprawny
    {
		Container c = new Container();
		c.RegisterType<Foo>( true );
		Foo f1 = c.Resolve<Foo>();
		Foo f2 = c.Resolve<Foo>();
        Assert.True(f1.Equals(f2), "f1 and f2 should be the same object");
    }

    [Fact]
    public void Test2()     //brak singletonu poprawny
    {
		Container c = new Container();
		c.RegisterType<Foo>( false );
		Foo f1 = c.Resolve<Foo>();
		Foo f2 = c.Resolve<Foo>();
        Assert.False(f1.Equals(f2), "f1 and f2 should not be the same object");
    }

    [Fact]
    public void Test3()     //wybór implementacji poprawny
    {
		Container c = new Container();
		c.RegisterType<IFoo, Foo>( false );
		IFoo f1 = c.Resolve<IFoo>();
		Assert.True(f1.GetType().Equals(typeof(Foo)), "f1 should be of type Foo");
    }

    [Fact]
    public void Test4()     //wyjątki poprawne
    {
		Container c = new Container();
        Assert.Throws<NotRegisteredDependency>(() => c.Resolve<IFoo>());
    }

    [Fact]
    public void Test5()     //zmiana wyboru implementacji poprawna
    {
		Container c = new Container();
		c.RegisterType<IFoo, Foo>( false );
		IFoo f1 = c.Resolve<IFoo>();
        c.RegisterType<IFoo, OtherFoo>( false );
		IFoo f2 = c.Resolve<IFoo>();
		Assert.False(f1.GetType().Equals(f2.GetType()), "f1 and f2 should be of different types");
    }
}

}
