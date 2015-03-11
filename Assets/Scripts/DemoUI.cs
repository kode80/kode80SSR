using UnityEngine;
using System.Collections;

public class DemoUI : MonoBehaviour 
{
	public ReflectionProbe[] reflectionProbes;
	public SSR screenspaceReflections;

	private bool reflectionProbesEnabled;
	private bool ssrEnabled;

	// Use this for initialization
	void Start () 
	{
		reflectionProbesEnabled = true;
		ssrEnabled = true;
	}

	void OnGUI()
	{
		bool oldValue = ssrEnabled;
		ssrEnabled = GUI.Toggle( new Rect( 10, 10, 200, 20), ssrEnabled, "Screen Space Reflections");
		if( oldValue != ssrEnabled)
		{
			screenspaceReflections.enabled = ssrEnabled;
		}

		if( reflectionProbes.Length > 0)
		{
			oldValue = reflectionProbesEnabled;
			reflectionProbesEnabled = GUI.Toggle( new Rect( 10, 40, 200, 20), reflectionProbesEnabled, "Reflection Probes");
			if( oldValue != reflectionProbesEnabled)
			{
				foreach( ReflectionProbe probe in reflectionProbes)
				{
					probe.enabled = reflectionProbesEnabled;
				}
			}
		}
	}
}
