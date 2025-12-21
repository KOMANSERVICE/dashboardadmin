using Microsoft.AspNetCore.Components;

namespace FrontendAdmin.Shared.Services;

/// <summary>
/// Interface d'abstraction pour les composants graphiques.
/// Permet de rendre des graphiques de manière cross-platform (Web et MAUI).
/// </summary>
/// <typeparam name="T">Type des données à afficher dans le graphique</typeparam>
public interface IGraphComponent<T>
{
    /// <summary>
    /// Génère un RenderFragment pour afficher le graphique avec les données fournies.
    /// </summary>
    /// <param name="items">Collection des données à afficher</param>
    /// <returns>Un RenderFragment qui peut être intégré dans un composant Blazor</returns>
    RenderFragment Render(IEnumerable<T> items);
}
