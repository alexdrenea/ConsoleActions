# Console Actions

Simple library that enhances productivity when writing a console app. 
One line set-up gets you going in seconds and you can use your console app as an interactive playground for the commands you define

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

Each method you want to participate in the action need to follow these rules:
 - be annotared with the `[Action]` attribute
 - be `public`
 - be `void` or `async` and return `Task`
 - either have no parameters, or one `string` argument or one `string` and one `dynamic` arguments

## Declare actions
There are a few ways you can initialize your Action:

1. The simplest and most basic usage.
```csharp
[Action("a", "methodA"]
public void MethodA()
{
  Console.WriteLine($"Method 1 running.");
}
```
Let's dive into the example above:
 - `MethodA()` is a public void method on `Program()` which is the class we initialized the `ConsoleREPL` with.
 - The method is annotated with the `Action` attribute which is initialized with a `params string[]` which represent the keywords you need to type in the console app to call `MethodA()`
 - When running your console app, you can type either `a` or `methodA` to call your method.

1. Complete example defining all available parameters
```csharp
[Action("b", "methodB", Description = "Test method that does not need an input", DisplayOrder = 1, MeasureExexutionTime = true)]
public async Task MethodB(string input)
{
   Console.WriteLine($"MethodB running with {input}.");
}  
```
![calling MethodB()](https://res.cloudinary.com/alex-tech-blog/image/upload/v1564368172/Blog/2019.08/ConsoleActions-execute_edbodh.png)

Let's dive into the example above:
  - `MethodB()` is a public async Task method on `Program()` which is the class we initialized the `ConsoleREPL` with.
  - The method is annotated with the `Action` attribute which is initialized with a `params string[]` which represent the keywords you need to type in the console app to call `MethodB()`
  - the method takes a string parameter that will be contain the entire text that enters after the command keyword.
  - `Description` and `DisplayOrder` parameters are used to describe the method in the help session (see below)
  - `MeasureExecutionTime` is a bonus parameter. If set to true, the app will show the exeuction time in seconds after each executed method. Defaults to false.

### Action Parameters	

As of version 2.0 the Console Actions library supports declaring and pasring parameters, CLI style
To declare a parameter use the `[ActionParameter]` attribute next to your Action. An ActionParameter allow the engine to parse input that comes after the command, into meaningful data structures so that you don't need to write the parsing code and be able to use comlex arguments. 


Withing the declaration of an `ActionParameter` you can define the following:
- **parameter Name**: the first argument of the attribute. This parameter must be a valid C# property name since it will be the exposed back to you in the dynamic parsed parameters object.
- **parameter Tokens**: the next arguments of the attribute. This is a collection of adressing tokens you can use to reference your argument in your input command. These tokens must follow basic CLI rules: must start with either "-" or "--". If it starts with "-" it must be one character. These parameters are optional. If not tokens are specified, two will get created for each parameter: one with the first letter of the parameter, one with the full name of the parameter. NOTE: if multiple parameters are declared to an action, starting with the same letter and no tokens are provided, the action will fail to be generated.
- **type**: Specifies the type of the parsed argument. Currently, the ActionParameter parsing engine supports `string`, `bool`, numerics and `DateTime` parameters. The default value is `string`
- **defaultValue**: Used to specify the default value of the parameter if it's not present in the input command. defaults to default(type)


## Declare parameters
Let's look at a few ways to declare your parameters:

1. Basic usage with explicit declarations
```csharp
[Action("d", "methodD")]
[ActionParameter("fileName", "-f","--fileName", Type = typeof(string), DefaultValue = ""))]
public void MethodD(string input, dynamic parsedInput)
{
    "MethodD".WriteInLine(ConsoleColor.DarkCyan);
    Console.WriteLine($"called with input='{input}'. Parsed into: file='{parsedInput.fileName}'");
}
```
In the exmple above, `MethodD` declares one parameter.
- The parameterName is `fileName`. That will be the way to accessed the parsed parameter in the parsedInput that gets passed to your method
- The way to referece your parameter when running your action can be done either with "-f" or "--fileName".
- The type of the parameter is string
- DefaultValue is empty string

2. Basic usage with defaults
```csharp
[Action("e", "methodE")]
[ActionParameter("fileName"))]
public void MethodE(string input, IDictionary<string,object> parsedInput)
{
    "MethodE".WriteInLine(ConsoleColor.DarkCyan);
    Console.WriteLine($"called with input='{input}'. Parsed into: file='{parsedInput["fileName"]}'");
}
```
MethodE above, is identical with MethodD. Specifically, note the inferred tokens for the parameter are still going to be "-f" or "--fileName". Note how the `parsedInput` parameter can also be declared as a Dictionary instead of dynamic. This looses the strong typing that dynamic offers and you might neeed to cast your values. The cast is guaranteed to work. 

Exceptions would be thown by the parsing engine before calling your method if the parameters provided are invalid according to their declared types.

3. Multiple parameters on an action
```csharp
[Action("f", "methodF", DisplayOrder = 4, MeasureExexutionTime = true)]
[ActionParameter("fileName", "-f", "--file")]
[ActionParameter("keepOpen", Type = typeof(bool), DefaultValue = true)]
[ActionParameter("recordsToRead", Type = typeof(int), DefaultValue = 1000)]
public void MethodF(string input, dynamic parsedInput)
{
    "MethodF".WriteInLine(ConsoleColor.DarkCyan);
    Console.WriteLine($"called with input='{input}'. Parsed into: file='{parsedInput.fileName}' and keepOpen={parsedInput.keepOpen} and recordsToRead={parsedInput.recordsToRead}");
}
```

### Built-in method(s)

Currently there is only one built-in method - "help" that lists all available operations. Just type `?`, `h` or `help`:

![Help output](https://res.cloudinary.com/alex-tech-blog/image/upload/v1564368172/Blog/2019.08/ConsoleActions-help_tm9myc.png)

The method uses the optional Description and DisplayOrder parameters to show more information or usage for your method.

### Helpers

`ConsoleActions` also exposes some Console.Write extensions that allow you to have a nicer looking output from your methods by using colors (There are libraries out there that specialize in this and you can definitely use one of those, but if all you require is simple color you can certanly stick with what's offered here')

Look for `ConsoleHelpers` `WriteLine` and `WriteInLine` methods and extension methods.
