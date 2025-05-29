using System;
using System.Runtime.Serialization; // Necessário para a anotação [Serializable]

namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

[Serializable]
public class NotFoundException : DomainException // Herda de DomainException
{
    public NotFoundException() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    // Este construtor é importante para a serialização/desserialização da exceção
    protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}