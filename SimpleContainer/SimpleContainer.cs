using System;
using System.Reflection;
using System.Collections.Generic;

namespace ContainerNamespace {
public class NotRegisteredDependency : Exception { }

public class Container
{
	private Dictionary<Type, Type> dependencyMap;
	private HashSet<Type> singletonSet;
	private Dictionary<Type, object> instanceMapper;

	public Container() {
		this.dependencyMap = new Dictionary<Type, Type>();
		this.singletonSet = new HashSet<Type>();
		this.instanceMapper = new Dictionary<Type, object>();
	}

	public void RegisterType<T>( bool Singleton ) where T : class {
		Type t = typeof(T);
		RegisterType(t, t, Singleton);
	}

	public void RegisterType<From, To>( bool Singleton ) where To : From {
		Type from = typeof(From);
		Type to = typeof(To);
		RegisterType(from, to, Singleton);
	}

	private object Create(Type t) {
			ConstructorInfo? constructor =
				t.GetConstructor(
						BindingFlags.Public,
						System.Type.DefaultBinder,
						CallingConventions.Any,
						new Type[0],
						null);
			if(constructor == null) {
				Console.WriteLine("constructor of {} is null", t.Name);
			}
			object result = constructor?.Invoke(null);
			if(result == null) {
				Console.WriteLine("result of Create is null");
			}
			return result;
	}

	private void RegisterType(Type from, Type to, bool Singleton) {
		dependencyMap.Add(from, to);
		if (Singleton) {
			singletonSet.Add(from);
		} else {
			singletonSet.Remove(from);
			instanceMapper.Remove(from);
		}
	}

	public T Resolve<T>() {
		Type t = typeof(T);
		if(singletonSet.Contains(t)) {
			if (!instanceMapper.ContainsKey(t)) {
				instanceMapper.Add(t, Create(dependencyMap.GetValueOrDefault(t)));
			}
			return (T)instanceMapper.GetValueOrDefault(t, null);
		} else {
			return (T)Create(dependencyMap.GetValueOrDefault(t));
		}
	}
}

public class Program {
	interface IFoo {}
	class Foo : IFoo {};
    class OtherFoo : IFoo {};

	public static void Main(string[] args) {
		Container c = new Container();
		c.RegisterType<Foo>( true );
		Foo f1 = c.Resolve<Foo>();
		Foo f2 = c.Resolve<Foo>();
        Console.WriteLine("{}", f1.Equals(f2));
	}
}
}
