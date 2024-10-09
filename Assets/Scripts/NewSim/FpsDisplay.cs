using UnityEngine;
using System.Collections;
using TMPro;

namespace ShepProject
{

	public class FpsDisplay : MonoBehaviour
	{
		TMP_Text fpss;
		public int counter = 0;
		public int current;

		private float maxMs;
		public float timer;

		private void Awake()
		{
			fpss = GetComponent<TMP_Text>();
		}

		void Update()
		{

			if (current > counter)
			{
				
				current = 0;

			}

			if (maxMs < (Time.unscaledDeltaTime) * 1000)
			{
				maxMs = (Time.unscaledDeltaTime) * 1000;
			}
			if (timer > 2)
			{
				timer = 0;
				maxMs = 0;
			}


			fpss.text = string.Format("{0:0.0} ms\n({1:0.} fps)\n{2:000.0}" + "ms",// \n" + ShepGM.inst.NPCS.npcEntitySystemData.quadTree.PositionCount,
				Time.unscaledDeltaTime * 1000.0f, 1.0f / Time.unscaledDeltaTime, maxMs);

			current++;
			timer += Time.deltaTime;
		}
	}


}