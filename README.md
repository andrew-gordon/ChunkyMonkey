# ChunkyMonkey - C# Object Chunking Source Generator

## Introduction

![ChunkyMonkey](media/ChunkMonkey.png)
ChunkyMonkey is a C# Code Generator to split a class containing list, array, collection or dictionary properties into chunks. It also provides the ability to merge the chunks back into a single instance.

## Use Cases

* You need to break down a large API request into smaller pieces - to avoid breaking the maximum request body size limit of your web server.

<br>

## Walkthrough

Imagine you have a class like this:

```csharp
public class CreateUserRequest
{
	public string Username { get; set; }
	public DateTime DateOfBirth { get; set; }	
	public List<int> FavouriteNumbers { get; set; }
	public string[] FavouriteFilms { get; set; }
	public Dictionary<string, string> Attributes { get; set; }
}
```

If a CreateUserRequest object is for a user with a lot of favourite numbers, films and attributes, you may want to split the object into chunks to reduce the size of the request object. _[Yes, this is a very contrived example!]_

**ChunkyMonkey** generates two methods within partial classes, where _T_ is the name of the class. 

> :memo: The method isn't generic. The generated code contains the actual class type rather than _T_.

``` csharp 
public IEnumerable<T> Chunk(int chunkSize)
```

``` csharp
public T MergeChunks(IEnumerable<T> chunks)
```

## ```IEnumerable<T> Chunk(int chunkSize)```

* Only partial classes can be chunked. This is because the code generates generates a partial class containing the methods above for each chunked class.
* Non-chunkable properties are repeated for each chunked instance.
* If a collection (list, collection, array or dictionary) property's values are exhausted, subsequent chunks will contain an empty collection for the property value.


### Example of usage

#### Input to `Chunks(3)`

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[1, 2, 3, 4, 5, 6, 7]` |
| FavouriteFilms | `['E.T.', 'Flight of the Navigator', 'Weird Science', 'Ferris Bueller’s Day Off', 'Ghostbusters']` |
| Attributes | ``` { ```<br> ```  'Eye Colour': 'Blue',```<br> ```  'Hair Colour': 'Brown',```<br> ```  'Height (cm)': '180',```<br> ```   'First Language': 'English' ```<br> ``` } ``` |

#### Output for `Chunks(3)`

_Chunk #1_

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[1, 2, 3]` |
| FavouriteFilms | `['E.T.', 'Flight of the Navigator', 'Weird Science']` |
| Attributes | ``` { ```<br> ```  'Eye Colour': 'Blue',```<br> ```  'Hair Colour': 'Brown',```<br> ```  'Height (cm)': '180',```<br> ``` } ``` |

_Chunk #2_

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[4, 5, 6]` |
| FavouriteFilms | `['Ferris Bueller’s Day Off', 'Ghostbusters']` |
| Attributes | ``` { ```<br> ```  'First Language': 'English' ```<br> ```} ``` |

_Chunk #3_

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[7]` |
| FavouriteFilms | `[]` || 
| Attributes | ``` {} ``` |

<br>

## ``T MergeChunks(IEnumerable<T> chunks)``

This generated method merges a set of chunks back into a single instance. 

<br>

## Using within a .NET project

1. Add the ChunkyMonkey NuGet package to your C# project containing classes that you'd like to be able to generate the `Chunk` and `MergeChunks` methods.

    | Environment | Command |
    |-------------|---------|
    | .NET CLI | `dotnet add package Gord0.ChunkyMonkey.CodeGenerator` |
    | VS Package Manager Console | `NuGet\Install-Package Gord0.ChunkyMonkey.CodeGenerator` |
    | VS Package Manager Console | `<PackageReference Include="Gord0.ChunkyMonkey.CodeGenerator" Version="x.y.z" />` |

2. Add the `[Chunk]` attribute to a class for which you'd like to generate the `Chunk` and `MergeChunks` methods. 

    > :bulb: **Tip:** Be sure to define the class a `partial` class - otherwise, you will receive a compiler error when you build your project.

    ```csharp
    using ChunkyMonkey.Attributes;

    namespace TestProject
    {
        [Chunk]
        public partial class Person
        {
            public string[] PhoneNumbers { get; set; }
        }
    }
    ```

    > :memo: **Note:** If your classes/DTOs live in a separate project, you should add the `Gord0.ChunkyMonkey.Attributes` package to that project. This package provides the `ChunkAttribute`. It's kept in a seperate package to avoid your project having a dependency on the code generator.

3. Build your project.

4. To view the generated partial classes:
	- Expand the Dependencies nodes under your project in the Solution Explorer. 
	- Expand the Analyzers node.
	- Expand the ChunkyMonkey.CodeGenerator node.
	- Expand the ChunkyMonkey.CodeGenerator.ChunkyMonkeyGenerator node.
	- Now you will see the generated partial classes, each containing the `Chunk` and `MergeChunks` methods. The generated classes are called `<ClassName>_Chunk.g.cs`
	
5. The output for the above Person class would be a file called `Person_Chunked.g.cs`:

    ```csharp
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    namespace TestProject
    {
        public partial class Person
        {
            /// <summary>
            /// Chunks the instance into multiple instances based on the specified chunk size.
            /// </summary>
            /// <param name="chunkSize">The size of each chunk.</param>
            /// <returns>An enumerable of chunked instances.</returns>
            public IEnumerable<Person> Chunk(int chunkSize)
            {
                int maxCollectionLength = 0;
                if (this.PhoneNumbers.Length > maxCollectionLength)
                {
                    maxCollectionLength = this.PhoneNumbers.Length;
                }

                for (int i = 0; i < maxCollectionLength; i += chunkSize)
                {
                    var instance = new Person();
                    {
                        if (this.PhoneNumbers is not null)
                        {
                            instance.PhoneNumbers = this.PhoneNumbers.Skip(i).Take(chunkSize).ToArray();
                        }
                    }


                    yield return instance;
                }
            }

            /// <summary>
            /// Merges the specified chunks into a single instance.
            /// </summary>
            /// <param name="chunks">The chunks to merge.</param>
            /// <returns>The merged instance.</returns>
            public static Person MergeChunks(IEnumerable<Person> chunks)
            {
                var instance = new Person();

                foreach(var chunk in chunks)
                {

                    if (chunk.PhoneNumbers is not null)
                    {
                        if (instance.PhoneNumbers is null)
                        {
                            instance.PhoneNumbers = Array.Empty<string>();
                        }

                        instance.PhoneNumbers = instance.PhoneNumbers.Concat(chunk.PhoneNumbers).ToArray();
                    }

                }

                return instance;
            }
       }
    }
    ```

<br>

## Limitations

* Does not currrently support nullable types.

<br>

## Future Enhancements

* Handle nullable reference types
* Check Chunk method doesn't already exist
* Check MergeChunks method doesn't already exist
* Check that the existing class is not sealed (if so, compiler warning)
* Check that the existing class is not static (if so, compiler warning)
* Check that the existing class is not abstract (if so, compiler warning)
* Check that the existing class has a parameterless constructor (if not, compiler warning)
* Check that the existing class has a public constructor (if not, compiler warning)
