using System;

namespace SimpleContainer;

public abstract class IDependency {
	protected IDependency next;
	public abstract T Resolve<T>() where T : new() ;
	public abstract IDependency UnRegister<T>();
}

public class DirectDependency<T> : IDependency {
	public DirectDependency(IDependency next) {
		base.next = next;
	}
   	public override U Resolve<U>() {
		if (typeof(U).Equals(typeof(T))) {
			return new U();
		} else {
			return next.Resolve<U>();
		}
	}

   	public override IDependency UnRegister<U>() {
		if (typeof(U).Equals(typeof(T))) {
			return next;
		} else {
			next = next.UnRegister<U>();
			return this;
		}
	}
}

public class InDirectDepencency<From, To> : IDependency {
	public InDirectDependency(IDependency next) {
		base.next = next;
	}
   	public override U Resolve<U>() where U : new() {
		if (U.Equals(T)) {
			return new U();
		} else {
			return next.Resolve<U>();
		}
	}
};

public class SingletonDependency<T> : IDependency {

};

public class NotRegisteredDependency : Exception { }

public class NotFoundDependency : IDependency {
	public NotFoundDependency() {
		base.next = null;
	}
	public override T Resolve<T>() {
		throw new NotRegisteredDependency();
	}
};





public class SimpleContainer
{
	private IDependency dependencyHead = new NotFoundDependency();

	public void RegisterType<T>( bool Singleton ) where T : class {

	}

	public void RegisterType<From, To>( bool Singleton ) where To : From {

	}

	public T Resolve<T>() {
		return dependencyHead.Resolve<T>();
	}
}
