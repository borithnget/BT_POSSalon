using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc.Async;
using System.Web.Mvc;
using System;
using Unity;

public class UnityDependencyResolver : IDependencyResolver
{
	private readonly IUnityContainer _container;

	public UnityDependencyResolver(IUnityContainer container)
	{
		_container = container ?? throw new ArgumentNullException(nameof(container));
	}

	public object GetService(Type serviceType)
	{
		try
		{
			if (IsMvcInfrastructureService(serviceType))
			{
				return null; 
			}		
		return _container.Resolve(serviceType);
		}
		catch (ResolutionFailedException)
		{
			return null;
		}
	}
	public IEnumerable<object> GetServices(Type serviceType)
	{
		try
		{
			return _container.ResolveAll(serviceType);
		}
		catch (ResolutionFailedException)
		{
			return new List<object>();
		}
	}
	private bool IsMvcInfrastructureService(Type serviceType)
	{
		var mvcServices = new[]
		{
			typeof(ITempDataProviderFactory),
			typeof(ITempDataProvider),
			typeof(IActionInvokerFactory),
			typeof(IAsyncActionInvokerFactory),
			typeof(IActionInvoker),
			typeof(IAsyncActionInvoker)
		};
		return mvcServices.Contains(serviceType);
	}
}
