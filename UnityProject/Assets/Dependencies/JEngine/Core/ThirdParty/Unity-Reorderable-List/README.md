# Reorderable List

An attempt to mimic the ReorderableList within Unity while adding some extended functionality.

![screenshot](https://user-images.githubusercontent.com/6723783/45054643-70b46200-b042-11e8-874c-0d93a46e05a5.jpg)

## Installation

This extension can be installed using Unity package manager.  
`https://github.com/cfoulston/Unity-Reorderable-List.git`  

* Package manager UI  

	![screenshot](https://user-images.githubusercontent.com/6723783/72479980-c9554c80-37aa-11ea-8fd8-978d3fa860bd.png)

* Manifest

		{
    		"dependencies": {
        		"com.malee.reorderablelist": "https://github.com/cfoulston/Unity-Reorderable-List.git"
    		}
		}

* A standalone version is available under the [standalone](https://github.com/cfoulston/Unity-Reorderable-List/tree/standalone) branch, although this version is no longer maintained.  

## Features

* Drag and Drop references (like array inspector)
* Expandable items and list itself
* Multiple selection (ctrl/command, shift select)
* Draggable selection
* Context menu items (revert values, duplicate values, delete values)
* Custom attribute which allows automatic list generation for properties*
* Event delegates and custom styling
* Pagination
* Sorting (sort based on field, ascending and descending)
* Surrogates (Enable adding elements of a different type)

## Usage

There are two ways to use the ReorderableList
1. Create a custom Editor for your class and create a ReorderableList pointing to your serializedProperty
2. Create custom list class which extends from ReorderableArray<T>, assign [Reorderable] attribute above property (not class).

## Pagination

Pagination can be enabled in two ways:

1. With the [Reorderable] attribute:
	* `[Reorderable(paginate = true, pageSize = 0)]`
2. Properties of the ReorderableList:
	* `list.paginate`
	* `list.pageSize`

`pageSize` defines the desired elements per page. Setting `pageSize = 0` will enable the custom page size GUI

When enabled, the ReorderableList GUI will display a small section below the header to facilitate navigating the pages

![pagination](https://user-images.githubusercontent.com/6723783/45054642-701bcb80-b042-11e8-84e4-0886d23c83c9.jpg)

#### NOTE 
*Elements can be moved between pages by right-clicking and selecting "Move Array Element"*

## Surrogates

Surrogates can be created to facilitate adding Objects to a ReorderableList that don't match the ReorderableList type.
This can be achieved in two ways:

1. With the [Reorderable] attribute:
	* `[Reorderable(surrogateType = typeof(ObjectType), surrogateProperty = "objectProperty")]`
2. Property of the ReorderableList:
	* `list.surrogate = new ReorderableList.Surrogate(typeof(ObjectType), Callback);`

Check the `SurrogateTest` and `SurrogateTestEditor` examples for more information