using Raylib_cs;

namespace raylib_cs_utils.Core;

public static class AssetManager
{
    /// <summary>
    /// Internal storage using (Type, Path) as key to avoid collisions.
    /// </summary>
    private static readonly Dictionary<(Type type, string path), object> Assets = new();
    
    /// <summary>
    /// Maps asset types to their load functions.
    /// </summary>
    private static readonly Dictionary<Type, Func<string, object>> Loaders = new()
    {
        { typeof(Texture2D), path => Raylib.LoadTexture(path) },
        { typeof(Font), path => Raylib.LoadFont(path) },
        { typeof(Sound), path => Raylib.LoadSound(path) },
        { typeof(Model), path => Raylib.LoadModel(path) },
    };
    
    /// <summary>
    /// Maps asset types to their unload functions.
    /// </summary>
    private static readonly Dictionary<Type, Action<object>> Unloaders = new()
    {
        { typeof(Texture2D), obj => Raylib.UnloadTexture((Texture2D)obj) },
        { typeof(Font), obj => Raylib.UnloadFont((Font)obj) },
        { typeof(Sound), obj => Raylib.UnloadSound((Sound)obj) },
        { typeof(Model), obj => Raylib.UnloadModel((Model)obj) },
    };
    
    /// <summary>
    /// Static constructor to register default Raylib asset types.
    /// </summary>
    static AssetManager()
    {
        Register<Texture2D>(Raylib.LoadTexture, Raylib.UnloadTexture);
        Register<Font>(Raylib.LoadFont, Raylib.UnloadFont);
        Register<Sound>(Raylib.LoadSound, Raylib.UnloadSound);
        Register<Model>(Raylib.LoadModel, Raylib.UnloadModel);
    }
    
    /// <summary>
    /// Registers a loader and unloader for a specific asset type.
    /// </summary>
    /// <typeparam name="T">Asset type</typeparam>
    /// <param name="loader">Function to load asset from path</param>
    /// <param name="unloader">Function to unload asset</param>
    public static void Register<T>(
        Func<string, T> loader,
        Action<T> unloader
    ) where T : notnull
    {
        Loaders[typeof(T)] = path => loader(path)!;
        Unloaders[typeof(T)] = obj => unloader((T)obj);
    }
    
    /// <summary>
    /// Loads an asset using a registered loader. Returns cached instance if already loaded.
    /// </summary>
    /// <param name="path">File path to asset</param>
    /// <typeparam name="T">Asset type</typeparam>
    /// <returns>Loaded asset instance</returns>
    /// <exception cref="NotSupportedException">Thrown if no loader is registered for type</exception>
    /// <exception cref="InvalidOperationException">Thrown if loading fails</exception>
    public static T Load<T>(string path) where T : notnull
    {
        var key = (typeof(T), path);
        // lock for thread safety
        lock (Assets)
        {
            // Return if cached.
            if (Assets.TryGetValue(key, out var existing)) return (T)existing;
            
            if (!Loaders.TryGetValue(typeof(T), out var loader))
                throw new NotSupportedException($"No loader registered for type {typeof(T).Name}");
            
            // load and cache asset
            var asset = (T)loader(path);
            if (asset == null) throw new InvalidOperationException($"Failed to load asset at path: {path}");
            Assets[key] = asset;
            return asset;
        }
    }
    
    /// <summary>
    /// Loads an asset using a custom loader. Returns cached instance if already loaded.
    /// </summary>
    /// <param name="path">File path to asset</param>
    /// <param name="loader">Custom load function</param>
    /// <typeparam name="T">Asset type</typeparam>
    /// <returns>Loaded asset instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if loading fails</exception>
    public static T Load<T>(string path, Func<string, T> loader) where T : notnull
    {
        var key = (typeof(T), path);
        lock (Assets)
        {
            // Return if cached.
            if (Assets.TryGetValue(key, out var existing)) return (T)existing;
            
            // load and cache asset
            var asset = loader(path);
            if (asset == null) throw new InvalidOperationException($"Failed to load asset at path: {path}");
            Assets[key] = asset;
            return asset;
        }
    }
    
    /// <summary>
    /// Unloads a specific asset if it exists in the cache.
    /// </summary>
    /// <param name="path">File path of asset</param>
    /// <typeparam name="T">Asset type</typeparam>
    /// <exception cref="NotSupportedException">Thrown if no unloader is registered for type</exception>
    public static void Unload<T>(string path)
    {
        var key = (typeof(T), path);
        lock (Assets)
        {
            if (!Assets.TryGetValue(key, out var asset)) return; // return if not in cache

            UnloadInternal(asset);
            Assets.Remove(key);
        }
    }
    
    /// <summary>
    /// Unloads all cached assets.
    /// </summary>
    /// <remarks>
    /// Useful for shutdown or full scene reload.
    /// </remarks>
    public static void UnloadAll()
    {
        lock (Assets)
        {
            foreach (var asset in Assets.Values)
            {
                UnloadInternal(asset);
            }

            Assets.Clear();
        }
    }
    
    /// <summary>
    /// Internal method that dispatches to the correct unloader.
    /// </summary>
    /// <param name="asset">Asset instance</param>
    /// <exception cref="NotSupportedException">Thrown if no unloader exists for type</exception>
    private static void UnloadInternal(object asset)
    {
        var type = asset.GetType();
        
        if (!Unloaders.TryGetValue(type, out var unloader)) 
            throw new NotSupportedException($"No unloader registered for type {type.Name}");
        
        unloader(asset);
    }
}