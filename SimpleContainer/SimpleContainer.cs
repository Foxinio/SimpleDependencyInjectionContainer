using System;
using System.Reflection;
using System.Collections.Generic;

namespace SimpleContainer {
public class NotRegisteredDependency : Exception { }
public class NoAvailableConstructors : Exception { }
public class DependencyCycleDetected: Exception { }

public class Container
{
	private Dictionary<Type, Type> dependencyMap;
	private HashSet<Type> singletonSet;
	private Dictionary<Type, object> instanceMapper;
	private Dictionary<Type, ConstructorInfo> constructorMap;

	public Container() {
		this.dependencyMap = new Dictionary<Type, Type>();
		this.singletonSet = new HashSet<Type>();
		this.instanceMapper = new Dictionary<Type, object>();
		this.constructorMap = new Dictionary<Type, ConstructorInfo>();
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

	public void RegisterInstance<From>(object Instance) {
		Type from = typeof(From);
		Type to = Instance.GetType();
		RegisterType(from, to, true);
		instanceMapper.Remove(from);
		instanceMapper.Add(from, Instance);
	}

	private void RegisterType(Type from, Type to, bool Singleton) {
		dependencyMap.Remove(from);
		dependencyMap.Add(from, to);
		RegisterConstructor(to);
		if (Singleton) {
			singletonSet.Add(from);
		} else {
			singletonSet.Remove(from);
			instanceMapper.Remove(from);
		}
	}

	private void RegisterConstructor(Type t) {
		if(constructorMap.ContainsKey(t)) {
			Console.WriteLine("	Consturctor for type " + t.ToString() + " already registered");
			return;
		}
		ConstructorInfo c = GetConstructor(t);
		constructorMap.Add(t, c);
		foreach(var parameter in c.GetParameters()) {
			RegisterConstructor(parameter.GetType());
		}
	}

	private ConstructorInfo GetConstructor(Type t) {
		ConstructorInfo[] constructors = t.GetConstructors();
		Array.Sort(constructors, new ConstructorComparer());
		Console.WriteLine("	Found " + constructors.Length + " constructors for type " + t.ToString());
		Console.WriteLine("	type is Abstract: " + t.IsAbstract + " and is Interface: " + t.IsInterface + "\n");
		foreach(var c in constructors) {
			if (ConstructorMatches(c.GetParameters())) {
				return c;
			}
		}
		throw new NoAvailableConstructors();
	}

	private class ConstructorComparer : IComparer<ConstructorInfo> {
		int IComparer<ConstructorInfo>.Compare(ConstructorInfo? x, ConstructorInfo? y) {
			if (y == null) {
				return 1;
			}
			if (x == null) {
				return -1;
			}
			return x.GetParameters().Length - y.GetParameters().Length;
		}
	}

	private bool ConstructorMatches(ParameterInfo[] parameters) {
		foreach(ParameterInfo p in parameters) {
			Type pType = p.GetType();
			if (!dependencyMap.ContainsKey(pType) &&
					(pType.IsAbstract || pType.IsInterface)){
				return false;
			}
		}
		return true;
	}






	public T Resolve<T>() {
		Type t = typeof(T);
		Console.WriteLine("\tStarting resolving request for type " + t);
		return (T)Resolve(t, new HashSet<Type>());
	}

	private object Resolve(Type t, HashSet<Type> beingConstructed) {
		// RAII check if 
		if (beingConstructed.Contains(t)) {
			throw new DependencyCycleDetected();
		}
		beingConstructed.Add(t);
		object result = ResolveInternal(t, beingConstructed);
		beingConstructed.Remove(t);
		return result;
	}

	private object ResolveInternal(Type t, HashSet<Type> beingConstructed) {
		Type outT;
		if (dependencyMap.TryGetValue(t, out outT)) {
			if(singletonSet.Contains(t)) {
				object result;
				if (!instanceMapper.TryGetValue(t, out result)) {
					result = Create(outT, beingConstructed);
					instanceMapper.Add(t, result);
				}
				return result;
			} else {
				return Create(outT, beingConstructed);
			}
		} else if (!t.IsAbstract && !t.IsInterface) {
			return Create(t, beingConstructed);
		} else {
			throw new NotRegisteredDependency();
		}
	}

	private object Create(Type t, HashSet<Type> beingConstructed) {
		ConstructorInfo? c = constructorMap.GetValueOrDefault(t);
		if (c == null) {
			return GetSimpleConstructor(t).Invoke(null);
		}
		object[] parameters = ResolveParameters(c.GetParameters(), beingConstructed);
		return c.Invoke(parameters);
	}

	private ConstructorInfo GetSimpleConstructor(Type t) {
		ConstructorInfo[] constructors = t.GetConstructors();
		Console.WriteLine("\tTrying SimpleConstructor");
		Console.WriteLine("	Found " + constructors.Length + " constructors for type " + t.ToString());
		Console.WriteLine("	type is Abstract: " + t.IsAbstract + " and is Interface: " + t.IsInterface);
		foreach(var c in constructors) {
			if (c.GetParameters().Length == 0) {
				return c;
			}
		}
		throw new NoAvailableConstructors();
	}

	private object[] ResolveParameters(ParameterInfo[] parameters, HashSet<Type> beingConstructed) {
		object[] result = new object[parameters.Length];
		for(int i = 0; i < parameters.Length; i++) {
			result[i] = Resolve(parameters[i].GetType(), beingConstructed);
		}
		return result;
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
        Console.WriteLine("	{}", f1.Equals(f2));
	}
}

}
