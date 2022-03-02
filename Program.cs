using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bogus;

BenchmarkRunner.Run(typeof(SerializerMethods).Assembly);

[MemoryDiagnoser]
public class SerializerMethods
{
    private MemoryStream? _memoryStream;
    private Utf8JsonWriter? _jsonWriter;
    private Pessoa[]? _pessoas;

    [GlobalSetup]
    public void Setup()
    {
        _memoryStream = new MemoryStream();
        _jsonWriter = new Utf8JsonWriter(_memoryStream);

        _pessoas = DataFakePeople.GetPessoas();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _memoryStream?.Dispose();
        _jsonWriter?.Dispose();
    }
     
    [Benchmark]
    public void SerializerDefault()
    {
        JsonSerializer.Serialize(_jsonWriter, _pessoas);

        _memoryStream.SetLength(0);
        _jsonWriter.Reset();
    }

    [Benchmark]
    public void SerializerCustom()
    {
        JsonSerializer.Serialize(_jsonWriter, _pessoas, PessoaCustomContext.Default.PessoaArray);

        _memoryStream.SetLength(0);
        _jsonWriter.Reset();
    }
}

[JsonSerializable(typeof(Pessoa[]))]
internal partial class PessoaCustomContext : JsonSerializerContext { }

internal static class DataFakePeople
{
    public static Pessoa[] GetPessoas()
    {
        return Enumerable.Range(1, 1000).Select(i =>
        {
            return new Faker<Pessoa>("pt_BR")
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.NomeCompleto, f => f.Name.FullName())
                .RuleFor(c => c.DataNascimento, f => f.Date.Past(20))
                .Generate();
        }
        ).ToArray();
    }
}

public record Pessoa
{
    public Guid Id { get; set; }
    public string? NomeCompleto { get; set; }
    public DateTime DataNascimento { get; set; }
}