using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace TestSTP
{
    public class SpriteManager : Singleton<SpriteManager> {
	    

		[SerializeField] private Sprite empty;
		
		[SerializeField] private SpriteAtlas _gameItemAtlas;

		
		#region Public Functions
		
		
		public Sprite GetItemSprite(string imgName) {
			var sprite = _gameItemAtlas.GetSprite(imgName);
			if (sprite) {
				return sprite;
			}

			Debug.Log($"Could not found icon with name = {imgName}");
			return empty;
		}
		
		#endregion
		
	}
}