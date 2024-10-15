# ChunkyMonkey - C# Object Chunking Source Generator

## Introduction

ChunkyMonkey is a C# Code Generator to split a class with list, array, collection or dictionary properties into chunks, and provides the ability to merge the chunks back into a single instance.

## Use Cases

* You need to break down a large API request into smaller pieces - to avoid breaking the maximum request body size limit of your web server.

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

If the user has a lot of favourite numbers, films and attributes, you may want to split the class into chunks to reduce the size of the request object. _[Yes, this is a very contrived example!]_

**ChunkyMonkey** generates two methods within partial classes, where _T_ is the name of the class. 

	> [!NOTE]  
	> The method isn't generic. The generated code contains the actual class type rather than _T_)

``` csharp 
public IEnumerable<T> Chunk(int chunkSize)
```

``` csharp
public T MergeChunks(IEnumerable<T> chunks)
```
## ```IEnumerable<T> Chunk(int chunkSize)```

* The original class must be a partial class.
* Non-chunkable properties are repeated for each chunked instance.
* If a collection (list, collection, array or dictionary) property's values are exhausted, subsequent chunks will contain an empty collection for the property value.


### Example of usage

#### Input to `Chunks(3)`

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[1, 2, 3, 4, 5, 6, 7]` |
| FavouriteFilms | `['E.T.', 'Flight of the Navigator', 'Weird Science', 'Ferris Bueller’s Day Off', 'Ghostbusters']` || Attributes | {<br>&nbsp;&nbsp;&nbsp;&nbsp;'Eye Colour': 'Blue'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Hair Colour': 'Brown'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Height (cm)': '180'<br>&nbsp;&nbsp;&nbsp;&nbsp;'First Language': 'English'<br>}

#### Output for `Chunks(3)`

_Chunk #1_

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[1, 2, 3]` |
| FavouriteFilms | `['E.T.', 'Flight of the Navigator', 'Weird Science']` |
| Attributes | {<br>&nbsp;&nbsp;&nbsp;&nbsp;'Eye Colour': 'Blue'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Hair Colour': 'Brown'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Height (cm)': '180'<br>&nbsp;&nbsp;&nbsp;&nbsp;'First Language': 'English'<br>}

_Chunk #2_

| Property | Value |
|--------|----------------|
| Username | 'bob' | 
| DateOfBirth | 1/2/1975 |
| FavouriteNumbers | `[4, 5, 6]` |
| FavouriteFilms | `['Ferris Bueller’s Day Off', 'Ghostbusters']` |
| Attributes | {<br>&nbsp;&nbsp;&nbsp;&nbsp;'Eye Colour': 'Blue'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Hair Colour': 'Brown'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Height (cm)': '180'<br>&nbsp;&nbsp;&nbsp;&nbsp;'First Language': 'English'<br>}

_Chunk #3_

| Property | Value |
|--------|----------------|
| Username | `'bob'` | 
| DateOfBirth | `1/2/1975` |
| FavouriteNumbers | `[7]` |
| FavouriteFilms | `[]` || Attributes | {<br>&nbsp;&nbsp;&nbsp;&nbsp;'Eye Colour': 'Blue'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Hair Colour': 'Brown'<br>&nbsp;&nbsp;&nbsp;&nbsp;'Height (cm)': '180'<br>&nbsp;&nbsp;&nbsp;&nbsp;'First Language': 'English'<br>}

## ``T MergeChunks(IEnumerable<T> chunks)``

This generated method merges a set of chunks back into a single instance. 

## Limitations

* Does not currrently support nullable types.
