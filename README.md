# TShockChestSort
A TShock plugin to sort items in chests

This plugin will automatically delete fishing trash and gravestones from sorted chests and attempt to move items between chests to organise them.
The chests in each TShock region are sorted leaving chests elsewhere alone.


## Installation
These instructions assume you already have [TShock](https://github.com/Pryaxis/TShock) installed. 
These instructions also assume you are using a pre-built version of the plugin. 
It is possible to compile one from the source code but that's beyond the scope of these instructions.

1. Download a pre-built copy of the plugin from [the releases](https://github.com/ToxicFrazzles/TShockChestSort/releases)
2. Open your TShock installation folder
3. Open the `ServerPlugins` folder and place the downloaded plugin in there


## Usage
The plugin only sorts chests that are in a TShock region. Specify a region using the `/region` commands in-game.

When closing a chest that is in a region, the chests will have their items sorted.

A config file is created when the plugin first attempts to sort items. 
This config file can be found in the TShock installation folder under `tshock/ChestSort`.
The config is a JSON file which allows you to define named chests and the categories of items that should be in said chest.

#### Commands
* `/sort`: This command forces the plugin to sort the chests in the same region as the chest you had open.
* `/pausesort`: This command temporarily stops the sorting of chests in the same region as the chest you have open.



## Known issues
* Unknown consequences of having TShock regions overlap
* No way to exclude a region from being sorted
* Potential crashes with incorrect config