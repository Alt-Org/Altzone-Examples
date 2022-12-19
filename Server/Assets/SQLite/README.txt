UnitySQLiteAsync

UnitySQLiteAsync is asynchronous SQLite-net support for Unity.

SQLite-net is simple, powerful, cross-platform SQLite client and ORM for .NET.
But sqlite-net's Asynchronous API is based on Threading, it's heavy and not matched to Unity threading (single-thread).
So I made Asynchronous API based on UniTask, which is more suitable for Unity.

Also Can use Synchronous API.

NOTE that everything is moved to namespace: SQLite

https://github.com/kdw9502/UnitySQLiteAsync