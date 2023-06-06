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






// lista 10
    [Fact]
    public void RegisterInstance()
    {
		Container c = new Container();
        IFoo f1 = new Foo();
        c.RegisterInstance<IFoo>(f1);
        IFoo f2 = c.Resolve<IFoo>();
        Assert.True(f1.Equals(f2), "f1 and f2 should be the same object");
    }

        [Fact]
    public void LastRegisteredInstance()
    {	
        Container c = new Container();
        IFoo f1 = new Foo();
        c.RegisterInstance<IFoo>(f1);
        IFoo f2 = new Foo();
        c.RegisterInstance<IFoo>(f2);
        IFoo f3 = c.Resolve<IFoo>();
        Assert.True(f3.Equals(f2), "f3 and f2 should be the same object");
        Assert.True(!f3.Equals(f1), "f3 and f1 should  not be the same object");
    }

    
    public class A {
        public B b;
        public A( B b ) {
            this.b = b;
        }
    };
    public class B { };
    public class C{
        public B b;
        public C(B b, string s) {
            this.b = b;}
    };

    [Fact]
    public void DependencyInjectionBasic()
    {
		Container c = new Container();
        A a = c.Resolve<A>();
        Assert.True(a.b != null, "it should properly create it");
    }
    
    [Fact]
    public void DependencyInjectionNoInstance()
    {
		Container c = new Container();
		Assert.Throws<NoAvailableConstructors>( () => c.Resolve<C>());
	}

[Fact]
public void DependencyInjectionWithInstance()
    {
		Container c = new Container();
        c.RegisterInstance<string>("aaaaaaaaaaaa");
        C a = c.Resolve<C>();
        Assert.True(a.b != null, "it should properly create the string");
    }

    
}

}
