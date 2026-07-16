namespace NovaERP.Application.Features.Authentication.Common;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// hash null (usuario inexistente): verifica igual contra un hash interno
    /// fijo, para que el costo de CPU de PBKDF2 sea el mismo exista o no la
    /// cuenta — evita que un atacante distinga "email no registrado" de
    /// "email registrado, password incorrecta" por el tiempo de respuesta.
    /// </summary>
    bool Verify(string password, string? hash);

    /// <summary>
    /// true si el hash usa parámetros viejos (ej. menos iteraciones PBKDF2 que
    /// el default actual del framework) y conviene regenerarlo. Solo tiene
    /// sentido llamarlo tras un Verify exitoso, que es el único momento en que
    /// se dispone de la password en claro para re-hashear.
    /// </summary>
    bool NeedsRehash(string password, string hash);
}
