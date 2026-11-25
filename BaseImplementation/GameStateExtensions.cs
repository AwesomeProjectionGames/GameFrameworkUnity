#nullable enable

using System;
using System.Collections.Generic;
using AwesomeProjectionCoreUtils.Extensions;
using GameFramework;

namespace GameFramework
{
    public static class GameStateExtensions
    {
        /// <summary>
        /// Resolves an actor prioritizing actorId (UUID) if specified, falling back to resourcePath.
        /// </summary>
        public static IActor? FindActor(this IGameState? gameState, string? actorId, string? resourcePath)
        {
            if (gameState == null) return null;

            if (!string.IsNullOrEmpty(actorId))
            {
                if (gameState.ActorsById != null && gameState.ActorsById.TryGetValue(actorId, out var actor) && actor.IsAlive())
                    return actor;
            }

            if (!string.IsNullOrEmpty(resourcePath))
            {
                return gameState.GetActorByResourcePath(resourcePath);
            }

            return null;
        }

        /// <summary>
        /// Gets the first alive actor matching the specified Resources-relative path.
        /// Note: This is an O(N) linear search over <see cref="IGameState.Actors"/>.
        /// </summary>
        // TODO: GetActorByResourcePath is O(N). If the actor count becomes large, consider maintaining an incremental reactive O(1) index in IGameState without per-frame rebuilds.
        public static IActor? GetActorByResourcePath(this IGameState? gameState, string? resourcePath)
        {
            if (gameState?.Actors == null || string.IsNullOrEmpty(resourcePath)) return null;

            foreach (var actor in gameState.Actors)
            {
                if (actor.IsAlive() && string.Equals(actor.ResourcePath, resourcePath, StringComparison.OrdinalIgnoreCase))
                    return actor;
            }

            return null;
        }

        /// <summary>
        /// Gets all alive actors matching the specified Resources-relative path.
        /// Note: This is an O(N) linear scan over <see cref="IGameState.Actors"/>.
        /// </summary>
        // TODO: GetActorsByResourcePath is O(N). If the actor count becomes large, consider maintaining an incremental reactive O(1) index in IGameState without per-frame rebuilds.
        public static IEnumerable<IActor> GetActorsByResourcePath(this IGameState? gameState, string? resourcePath)
        {
            if (gameState?.Actors == null || string.IsNullOrEmpty(resourcePath)) yield break;

            foreach (var actor in gameState.Actors)
            {
                if (actor.IsAlive() && string.Equals(actor.ResourcePath, resourcePath, StringComparison.OrdinalIgnoreCase))
                    yield return actor;
            }
        }
    }
}
