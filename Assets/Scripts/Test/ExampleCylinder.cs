///////////////////////////////////////////////////////////////////////////////
// Author: Federico Garcia Garcia
// License: GPL-3.0
// Created on: 04/06/2020 23:00
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ExampleCylinder : TubeGenerator
{
	Vector3[][] polylines = new Vector3[1][];
	void Start()
	{
		
		polylines[0] = new Vector3[4];
		polylines[0][0] = new Vector3(0, -1, 0);
		polylines[0][1] = new Vector3(0, 1, 0);
		polylines[0][2] = new Vector3(0, 1, 1);
		polylines[0][3] = new Vector3(-1, 1, 1);

		StartCoroutine(Move());
	}

	public IEnumerator Move()
    {
		yield return Generate(polylines);
		yield return new WaitForSeconds(.5f);
	}
}