using System.Collections.Frozen;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.ElementComparers;

namespace NexusMods.Abstractions.MnemonicDB.Analyzers;

/// <inheritdoc />
public class TreeAnalyzer : IAnalyzer<FrozenSet<EntityId>>
{
    /// <inheritdoc />
    public object Analyze(IDb? dbOld, IDb dbNew)
    {
        var remaining = new Stack<(EntityId E, bool IsRetract)>();
        
        dbOld ??= dbNew.Connection.AsOf(TxId.From(dbNew.BasisTxId.Value - 1));
        
        HashSet<EntityId> modified = new();
        
        foreach (var datom in dbNew.RecentlyAdded)
        {
            remaining.Push((datom.E, datom.IsRetract));
        }
        
        while (remaining.Count > 0)
        {
            var current = remaining.Pop();
            
            if (!modified.Add(current.E))
                continue;
            
            var db = current.IsRetract ? dbOld : dbNew;
            var entity = db.Get(current.E);
            foreach (var datom in entity)
            {
                if (datom.Prefix.ValueTag != ValueTag.Reference)
                    continue;

                var parent = ValueTag.Reference.Read<EntityId>(datom.ValueSpan);
                remaining.Push((parent, current.IsRetract));
            }
        }

        return modified.ToFrozenSet();
    }
}
