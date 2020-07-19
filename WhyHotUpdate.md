## What is hot update and why

As we acknowledged, when developers update their apps/games, users will have to download the latest version from app stores, this is what we called "cold-update". Hot-update, sounds like a antonym of normal update process, it is a way which developers upload their newest codes & resources into their web server, and since users open their apps/games, they automatically download the latest codes and resources from the server, and those codes and resources reloads, overrides the old versions.

It sounds simple right? But it is **actually not** easy to implement.

In the old days, we can use a technology which is called "JIT" to implement hot-update, but then App Store blocks this way as their hardware doesn't supports JIT, which means developers needs to find a new way to solve it out.

There are two main ways to implement hot-update:

1. Use Lua

   - Lua is a script language just like javascript, when Unity loads the game, this solution will make a virtual environment which runs those lua codes
   - Lua files are just like other text files, it ends with .lua and can be readed as TextAsset
   - You have to learn Lua first to use this solution

2. Use ILRuntime

   - ILRuntime is a solution which loads dll (What you can get after you build your c# soulution) in game and loads the methods in it

   - ILRuntime runs faster then lua except doing calculation
   - ILRuntime is written in c# and easy to use, that is the reason why JEngine is based on ILRuntime, not Lua