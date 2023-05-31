namespace SimpleContainer.Tests {

public class Tests
{
	interface IFoo {}
	class Foo : IFoo {};
    class OtherFoo : IFoo {};
    class BadFoo {
        public BadFoo(int i) {}        
    }

    [Fact]
    public void TestBasic()
    {
		Container c = new Container();
		Foo f1 = c.Resolve<Foo>();
        Assert.True(f1 != null, "f1 should not be null");
    }

    [Fact]
    public void TestBasicWithRegister()
    {
		Container c = new Container();
        c.RegisterType<Foo>( false );
		Foo f1 = c.Resolve<Foo>();
        Assert.True(f1 != null, "f1 should not be null");
    }

    [Fact]
    public void TestIncorrectConstructor()
    {
		Container c = new Container();
        Assert.Throws<NoAvailableConstructors>(() => c.Resolve<BadFoo>());
    }

    [Fact]
    public void TestSingleton()     //singleton poprawny
    {
		Container c = new Container();
		c.RegisterType<Foo>( true );
		Foo f1 = c.Resolve<Foo>();
		Foo f2 = c.Resolve<Foo>();
        Assert.True(f1.Equals(f2), "f1 and f2 should be the same object");
    }

    [Fact]
    public void TestNoSingleton()     //brak singletonu poprawny
    {
		Container c = new Container();
		c.RegisterType<Foo>( false );
		Foo f1 = c.Resolve<Foo>();
		Foo f2 = c.Resolve<Foo>();
        Assert.False(f1.Equals(f2), "f1 and f2 should not be the same object");
    }

    [Fact]
    public void TestInterfaceChoice()     //wybór implementacji poprawny
    {
		Container c = new Container();
		c.RegisterType<IFoo, Foo>( false );
		IFoo f1 = c.Resolve<IFoo>();
		Assert.True(f1.GetType().Equals(typeof(Foo)), "f1 should be of type Foo");
    }

    [Fact]
    public void TestMultipleChoice()     //zmiana wyboru implementacji poprawna
    {
		Container c = new Container();
		c.RegisterType<IFoo, Foo>( false );
		IFoo f1 = c.Resolve<IFoo>();
        c.RegisterType<IFoo, OtherFoo>( false );
		IFoo f2 = c.Resolve<IFoo>();
		Assert.False(f1.GetType().Equals(f2.GetType()), "f1 and f2 should be of different types");
    }

    [Fact]
    public void TestRegisteredDependencyException()
    {
		Container c = new Container();
        Assert.Throws<NotRegisteredDependency>(() => c.Resolve<IFoo>());
    }
}

}
