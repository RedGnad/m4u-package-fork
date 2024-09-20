using System.Collections.Generic;
using UnityEngine;

namespace Multisynq {


[SerializeField]
public class SynqVarPerPlayer<T> {
  
  string varId;
  Dictionary<string,T> values = new();

  public SynqVarPerPlayer(string _varId, T _myValue) {
    myValue = _myValue;
    varId = _varId;

    // subscribe to changes by other joined players in the session
  }

  public T myValue {
    get {
      return values[Mq_Bridge.Instance.croquetViewId];
    }
    set {
      values[Mq_Bridge.Instance.croquetViewId] = value;
    }
  }

  public T getValue( string playerId ) {
    return values[playerId];
  }

}

} // namespace MultisynqNS