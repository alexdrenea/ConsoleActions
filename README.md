# Console Actions

Simple library that enhances productivity when writing a console app. 
One line set-up gets you going in seconds and you can use your console app as an interactive playgorund for the commnds you define

### Getting started

To get started install the package from nuget (`nuget install ConsoleActions`) then update your Main class:

```csharp
static void Main(string[] args)
{
    try
    {
        new ConsoleActions.ConsoleREPL(new Program()).RunLoop().Wait();
    }
    catch (Exception e)
    {
        Console.WriteLine("Error: {0}", e.Message);
    }
    finally
    {
        Console.WriteLine("End of demo, press any key to exit.");
        Console.ReadKey();
    }
}
```

There's some extra exception handling code (that is not mandatory) but essentially it's the line in the `try` statement that is doing all the work.


![Running your app](https://res.cloudinary.com/alex-tech-blog/image/upload/v1564368172/Blog/2019.08/ConsoleActions-startup_uqqibx.png)


### Action methods
From here, all you need to do is to define what actions you can perform from your console app (this is the code that you'd usually write as methods and call them from Main, one after another or comment them out in between runs)

Each method you want to participate in the action, needs to be public, async, return a Task and take a string parameter and be annotated with the `[Action]` attribute.

```csharp
[Action("m", "method1", Description = "Test method", DisplayOrder = 100, MeasureExexutionTime = true)]
public async Task Method1(string parameter)
{
	Console.WriteLine($"Method 1 running with {parameter}.");
}
```

![calling Method1()](https://res.cloudinary.com/alex-tech-blog/image/upload/v1564368172/Blog/2019.08/ConsoleActions-execute_edbodh.png)

Let's dive into the example above:
 - `Method1()` is a public async Task method on `Program()` which is the class we initialized the `ConsoleREPL` with.
 - The method is annotated with the `Action` attribute which is initialized with a `params string[]` which represent the keywords you need to type in the console app to call Method1()
 - `Description` and `DisplayOrder` parameters are used to describe the method in the help session (see below)
 - 'MeasureExecutionTime' is a bonus parameter. If set to true, the app will show the exeuction time in seconds after each executed method.
 - the method takes a string parameter that will be whatever the user enters after the command keyword.

**Notes:**

The library does not offer a way to parse parameters at this time. If your method needs specifc paramters parameters, you will need to implement your own logic to parse those (i.e. when doing a data load, you might want to pass in how many entries you want to load.)

### Built-in method(s)

Currently there is only one built-in method - "help" that lists all available operations. Just type `?`, `h` or `help`:

![Help output](https://res.cloudinary.com/alex-tech-blog/image/upload/v1564368172/Blog/2019.08/ConsoleActions-help_tm9myc.png)


### Helpers

`ConsoleActions` also exposes some Console.Write extensions that allow you to have a nicer looking output from your methods by using colors (There are libraries out there that specialize in this and you can definitely use one of those, but if all you require is simple color you can certanly stick with what's offered here')

Look for `ConsoleHelpers` `WriteLine` and `WriteInLine` methods and extension methods.
