using Content.Pirate.Shared.IntegratedCircuits.Systems;

namespace Content.Pirate.Client.IntegratedCircuits;

/// <summary>
/// Клієнтська реалізація системи інтегралок.
/// Потрібна, щоб IoC в Robust знав, що підставляти в Shared-системи на стороні клієнта.
/// </summary>
public sealed class ClientIntegratedCircuitSystem : SharedIntegratedCircuitSystem
{
    // Тут можна нічого не писати, якщо клієнту не треба особливої логіки.
    // Всі методи з Shared будуть працювати автоматично.
}
