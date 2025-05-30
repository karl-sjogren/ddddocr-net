using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Karls.CaptchaReader.TestHelpers;

public static class Resources {
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public static string GetString(string name, Encoding? encoding = null) {
        return GetStringAsync(name, encoding).GetAwaiter().GetResult();
    }

    public static async Task<string> GetStringAsync(string name, Encoding? encoding = null) {
        var assembly = GetTestAssembly();
        await using var resourceStream = assembly!.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{name}");

        if(resourceStream == null) {
            throw new InvalidOperationException($"Failed to load resource stream {assembly.GetName().Name}.Resources." + name);
        }

        using var reader = new StreamReader(resourceStream, encoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public static Stream GetStream(string name) {
        return GetStreamAsync(name).GetAwaiter().GetResult();
    }

    public static async Task<Stream> GetStreamAsync(string name) {
        var assembly = GetTestAssembly();
        await using var resourceStream = assembly!.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{name}");

        if(resourceStream == null) {
            throw new InvalidOperationException($"Failed to load resource stream {assembly.GetName().Name}.Resources." + name);
        }

        var ms = new MemoryStream();
        await resourceStream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static T? GetTypedFromJson<T>(string name) {
        return GetTypedFromJsonAsync<T?>(name).GetAwaiter().GetResult();
    }

    public static async Task<T?> GetTypedFromJsonAsync<T>(string name, JsonSerializerOptions? options = null) {
        var assembly = GetTestAssembly();
        await using var resourceStream = assembly!.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{name}");

        if(resourceStream == null) {
            throw new InvalidOperationException($"Resource with {name} could not be found.");
        }

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(json, options ?? _serializerOptions);
    }

    internal static string[] GetResourcesNames() {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames();
    }

    internal static Assembly GetTestAssembly() {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();
        var assemblies = frames.Select(frame => frame.GetMethod()?.DeclaringType?.Assembly).Distinct();

        var assembly = assemblies.FirstOrDefault(assembly => {
            var assemblyName = assembly?.GetName()?.Name;

            if(string.IsNullOrEmpty(assemblyName)) {
                return false;
            }

            if(!assemblyName.StartsWith("Karls.CaptchaReader.", StringComparison.Ordinal)) {
                return false;
            }

            if(assemblyName == "Karls.CaptchaReader.TestHelpers") {
                return false;
            }

            return true;
        });

        if(assembly == null) {
            throw new InvalidOperationException("Failed to find test assembly.");
        }

        return assembly;
    }
}
