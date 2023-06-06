using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace SimpleContainer {
public class NotRegisteredDependency : Exception { }
public class NoAvailableConstructors : Exception { }
public class DependencyCycleDetected: Exception { }

public class Container
{
	public TextWriter Log = TextWriter.Null;

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
		dependencyMap.Remove(from);
		dependencyMap.Add(from, to);
		instanceMapper.Remove(from);
		singletonSet.Add(from);
		instanceMapper.Add(from, Instance);
	}

	private void RegisterType(Type from, Type to, bool Singleton) {
		dependencyMap.Remove(from);
		RegisterConstructor(to);
		dependencyMap.Add(from, to);
		if (Singleton) {
			singletonSet.Add(from);
			instanceMapper.Add(from, Create(to, new HashSet<Type>()));
		} else {
			singletonSet.Remove(from);
			instanceMapper.Remove(from);
		}
	}

	private void RegisterConstructor(Type t) {
		if(dependencyMap.ContainsKey(t)) {
			Log.WriteLine("	Dependency for type " +
					t.ToString() + " already registered, Constructor not needed");
			return;
		}
		Log.WriteLine("\tRegistering Constructor for type " + t);
		ConstructorInfo c = GetConstructor(t);
		constructorMap.Add(t, c);
		foreach(var parameter in c.GetParameters()) {
			RegisterConstructor(parameter.ParameterType);
		}
	}

	private ConstructorInfo GetConstructor(Type t) {
		ConstructorInfo[] constructors = t.GetConstructors();
		Array.Sort(constructors, new ConstructorComparer());
		Log.WriteLine("\tTrying GetConstructor");
		Log.WriteLine("	Found " + constructors.Length + " constructors for type " + t.ToString());
		Log.WriteLine("	type is Abstract: " + t.IsAbstract + " and is Interface: " + t.IsInterface + "\n");
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
			if (!dependencyMap.ContainsKey(p.ParameterType) &&
					!singletonSet.Contains(p.ParameterType) &&
					(p.ParameterType.IsAbstract || p.ParameterType.IsInterface)){
				Log.WriteLine("Constructor Matcher Failed for type " + p.ParameterType);
				return false;
			}
		}
		return true;
	}



	private class ResolveAntiCycle {
		Type t;
		HashSet<Type> set;
		public ResolveAntiCycle(Type t, HashSet<Type> set) {
			this.t = t;
			this.set = set;
			if(set.Contains(t)) {
				throw new DependencyCycleDetected();
			}
			set.Add(t);
		}
		~ResolveAntiCycle() {
			set.Remove(t);
		}
	}

	public T Resolve<T>() {
		Type t = typeof(T);
		if(!dependencyMap.ContainsKey(t)) {
			Log.WriteLine("\tRequested Type is not registered: " + t);
			if(t.IsAbstract || t.IsInterface) {
				throw new NotRegisteredDependency();
			}
			RegisterType(t, t, false);
		}
		Log.WriteLine("\tStarting resolving registered request for type " + t);
		return (T)Resolve(t, new HashSet<Type>());
	}

	private object Resolve(Type t, HashSet<Type> beingConstructed) {
		ResolveAntiCycle _ = new ResolveAntiCycle(t, beingConstructed);

		if(singletonSet.Contains(t)) {
			object result = instanceMapper.GetValueOrDefault(t);
			return result;
		}

		Type outT;
		if (dependencyMap.TryGetValue(t, out outT)) {
			return Create(outT, beingConstructed);
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
		Log.WriteLine("\tTrying SimpleConstructor");
		Log.WriteLine("	Found " + constructors.Length + " constructors for type " + t.ToString());
		Log.WriteLine("	type is Abstract: " + t.IsAbstract + " and is Interface: " + t.IsInterface);
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
			result[i] = Resolve(parameters[i].ParameterType, beingConstructed);
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
