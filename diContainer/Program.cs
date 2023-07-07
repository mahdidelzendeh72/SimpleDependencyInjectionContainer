using System.Reflection;
///This is a simplified implementation of a DI container
///and does not include advanced features like
///constructor injection,
///lifetime management,
///or dependency graph resolution.
///However, it provides a basic understanding of how a DI container
///can be implemented in C#.

public interface IContainer
{
    void Register<TInterface, TImplementation>();
    TInterface Resolve<TInterface>();
}

public class Container : IContainer
{
    private readonly Dictionary<Type, Type> _registeredTypes;

    public Container()
    {
        _registeredTypes = new Dictionary<Type, Type>();
    }

    public void Register<TInterface, TImplementation>()
    {
        _registeredTypes[typeof(TInterface)] = typeof(TImplementation);
    }
    private object Resolve(Type type)
    {
        MethodInfo resolveMethod = typeof(Container).GetMethod("Resolve");
        MethodInfo genericResolveMethod = resolveMethod.MakeGenericMethod(type);
        return genericResolveMethod.Invoke(this, null);
    }
    public TInterface Resolve<TInterface>()
    {
        Type implementationType = _registeredTypes[typeof(TInterface)];
        ConstructorInfo constructor = implementationType.GetConstructors().FirstOrDefault();

        if (constructor == null)
        {
            throw new InvalidOperationException($"No constructor found for type {implementationType.Name}.");
        }

        ParameterInfo[] parameters = constructor.GetParameters();
        object[] resolvedParameters = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            resolvedParameters[i] = Resolve(parameters[i].ParameterType);
        }

        return (TInterface)constructor.Invoke(resolvedParameters);
    }
}

// Example usage
public interface ILogger
{
    void Log(string message);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine("Logging: " + message);
    }
}

public class UserService
{
    private readonly ILogger _logger;

    public UserService(ILogger logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.Log("Doing something...");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create an instance of the DI container
        IContainer container = new Container();

        // Register the dependencies
        container.Register<ILogger, ConsoleLogger>();
        container.Register<UserService, UserService>();

        // Resolve the UserService from the container
        UserService userService = container.Resolve<UserService>();

        // Use the resolved UserService
        userService.DoSomething();
    }
}
