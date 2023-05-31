using System;
using System.Reflection;
using System.Collections.Generic;

namespace SimpleContainer {
public class NotRegisteredDependency : Exception { }
public class NoAvailableConstructors : Exception { }

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

	private ConstructorInfo GetConstructor(Type t) {
		ConstructorInfo[] constructors = t.GetConstructors();
		foreach(var c in constructors) {
			if (c.GetParameters().Length == 0) {
				return c;
			}
		}
		throw new NoAvailableConstructors();
	}

	private object Create(Type t) {
		return GetConstructor(t).Invoke(null);
	}

	private void RegisterType(Type from, Type to, bool Singleton) {
		dependencyMap.Remove(from);
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
		Type outT;
		if (dependencyMap.TryGetValue(t, out outT)) {
			if(singletonSet.Contains(t)) {
				object result;
				if (!instanceMapper.TryGetValue(t, out result)) {
					result = Create(outT);
					instanceMapper.Add(t, result);
				}
				return (T)result;
			} else {
				return (T)Create(outT);
			}
		} else if (!t.IsAbstract && !t.IsInterface) {
			return (T)Create(t);
		} else {
			throw new NotRegisteredDependency();
		}
	}

	public void RegisterInstance<T>(T Instance) {
		Type t = typeof(T);
		RegisterType(t, t, true);
		instanceMapper.Remove(t);
		instanceMapper.Add(t, Instance);
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
