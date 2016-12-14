using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CFGRuleGenerator : MonoBehaviour {

	private Dictionary <string, string[]> cfgRules; // CFG rules for LS rule generation

	public void Init() {
		cfgRules = new Dictionary <string, string[]> ();

		// Chain of one or more positive symbols
		string[] cfOp = new string[] {"+(CS')", "&(CS')"};
		cfgRules.Add ("(CS)", cfOp);

		// Chain of zero or more positive symbols
		string[] cfOpN = new string[] {"", "+(CS')", "&(CS')"};
		cfgRules.Add ("(CS')", cfOpN);

		// Chain of one or more negative symbols
		string[] cfOpG = new string[] {"-(CS')", "^(CS')"};
		cfgRules.Add ("(-CS)", cfOpG);

		// Chain of zero or more negative symbols
		string[] cfOpNG = new string[] {"", "-(CS')", "^(CS')"};
		cfgRules.Add ("(-CS')", cfOpNG);

		// These generate a string of one or more of the character
		string[] multi_pls = new string[] { "+(+')" };
		string[] multi_mns = new string[] { "-(-')" };
		string[] multi_amp = new string[] { "&(&')" };
		string[] multi_crt = new string[] { "^(^')" };
		string[] multi_exc = new string[] { "!(!')" };
		cfgRules.Add ("(MLT+)", multi_pls);
		cfgRules.Add ("(MLT-)", multi_mns);
		cfgRules.Add ("(MLT&)", multi_amp);
		cfgRules.Add ("(MLT^)", multi_crt);
		cfgRules.Add ("(MLT!)", multi_exc);
		cfgRules.Add ("(MLT*)", new string[] {"(MLT+)", "(MLT-)", "(MLT&)", "(MLT^)"});

		// These generate a string of zero or more of the character
		string[] myb_pls = new string[] { "", "+(+')" };
		string[] myb_mns = new string[] { "", "-(-')" };
		string[] myb_amp = new string[] { "", "&(&')" };
		string[] myb_crt = new string[] { "", "^(^')" };
		string[] myb_exc = new string[] { "", "!(!')" };
		cfgRules.Add ("(+')", myb_pls);
		cfgRules.Add ("(-')", myb_mns);
		cfgRules.Add ("(&')", myb_amp);
		cfgRules.Add ("(^')", myb_crt);
		cfgRules.Add ("(!')", myb_exc);

		cfgRules.Add ("(ANY*)", new string[] { "+", "&", "-", "^" });

		// Filler, will be determined by each unique plant.
		string[] br_intnodes = new string[] { };
		string[] draw_intnodes = new string[] { };
		string[] blank_intnodes = new string[] { };
		string[] branch_types = new string[] { };
		cfgRules.Add ("(BrIn)", br_intnodes);
		cfgRules.Add ("(DrIn)", draw_intnodes);
		cfgRules.Add ("(BlkIn)", blank_intnodes);
		cfgRules.Add ("(BrTyp)", branch_types);

		// Turn segement - change of angle and a draw of the unit, decrease size of next seg
		string[] turn_seg = new string[] { "(CS)F!(BrIn)", "(CS)(MLTF)!(BrIn)", 
			"(CS)F!(BrTyp)", "(CS)(MLTF)!(BrTyp)"};
		cfgRules.Add ("(TSeg)", turn_seg);

		string[] turn_seg_chain = new string[] { "", "(TSeg)(TSeg')" };
		cfgRules.Add ("(TSeg')", turn_seg_chain);

		string[] draw_templates = new string[] { 
			"(MLT*)(Dr1)(Drk)",
			"(Dr1')(Drk)(BrTyp)"
		};
		cfgRules.Add ("(DrT)", draw_templates);


		string[] branch_templates = new string[] {
			"(Dr1')[(MLT&)(MLT+)!(BrTyp)(ANY*)l](Dr1')[(MLT^)(MLT-)!(BrTyp)(ANY*)l]!(BrTyp)(ANY*)l",
			"(Dr1')[(MLT^)(MLT+)!(BrTyp)(ANY*)l](Dr1')[(MLT&)(MLT-)!(BrTyp)(ANY*)l]!(BrTyp)(ANY*)l"
		};
		cfgRules.Add ("(BrT)", branch_templates);

		//string[] branch_internode = new string[] { "[(TSeg)]" };

		// Draw 1 unit
		cfgRules.Add ("(Dr1)", new string[] { "(DrIn)" });
		cfgRules.Add ("(Dr1')", new string[] { "", "(DrIn)" });

		//Draw at least 1 unit
		//cfgRules.Add ("(MTL)", new string[] { "F(Dr_k')" });
		cfgRules.Add ("(Drk)", new string[] { "(Dr1)(Drk')" });
		cfgRules.Add ("(Drk')", new string[] { "", "(Dr1)(Drk')" });

		cfgRules.Add ("(MLTF)", new string[] { "F(MLTF')" });
		cfgRules.Add ("(MLTF')", new string[] { "", "F(MLTF')" });

	}


	// Before rule generation, can remove certain branching types
	// or internode types depending on what variables generated for
	// the plant

	/* Use this for setting (BrIn), (DrIn), (BlkIn), (BrTyp) once we determine
	 * how many of each type of node will exist.
	 * */
	public void AddCFGRule (string key, string[] rule) {
		cfgRules[key] = rule;
	}


	public string GenerateRule (char letter, int var1) {

		string ruleStr = "";

		if (letter >= 'A' && letter <= 'E') { // Branching internode
			// [(TSeg)] var1 of these separated with variable-length &s
			// [(TSeg)]&&(MLT&)[(TSeg)]&&(MLT&)  etc...
			string tSegStr = "[(TSeg)]";
			string turnStr = "&&&(MLT&)";
			string brInStr = tSegStr;
			for (int i = 0; i < var1 - 1; i++) {
				brInStr += turnStr + tSegStr;
			}
			ruleStr = ExpandString (brInStr, 0, 4);
		} else if (letter >= 'F' && letter <= 'K') { // Expand the draw
			//1: expand, e.g. +FF
			//2: branch off, e.g. F[+!S]F[-!S]!S



			ruleStr = ExpandString ("(DrT)", 0, 2); //TODO - 
			// Other possibilities: have branching from the F, e.g. [+!S][-!S]FF
		} else if (letter >= 'S' && letter <= 'X') { //Specific type of branch
			// (Dr)[(MLT+)(BrTy)][(MLT-)(BrTyp)] //draw F or G?
			ruleStr = ExpandString ("(BrT)", 0, 4);
		}


		return ruleStr;
	}



	private string ExpandString (string str, int currentDepth, int maxDepth) {
		Vector2 inds;
		int j = -1; //the indices of the last ( and ), respectively
		string expansion = "";

		while ((inds = ParseParan(str, j + 1)).y != -1) { //return next ( index-of-(, index-of-) )
			expansion += str.Substring (j + 1, (int)inds.x - (j + 1)); // Add the gap of 'real' things
			expansion += ExpandParan (str.Substring ((int)inds.x, (int)inds.y - (int)inds.x + 1), currentDepth, maxDepth);
			j = (int)inds.y;
		}

		// Add the rest of the string, from i onward
		if (j + 1 < str.Length) {
			expansion += str.Substring (j + 1);
		}
		return expansion;
	}


	private string ExpandParan (string key, int currentDepth, int maxDepth) {
		if (currentDepth >= maxDepth && cfgRules [key] [0] == "") { // Force recursion to end
			return "";
		}

		// Randomly select one of the rules
		// In future should do this by assigning probabilities

		string[] rules = cfgRules [key];
		int index = Random.Range (0, rules.Length);
		string template = rules [index];

		return ExpandString (template, currentDepth + 1, maxDepth);
	}

	// Return the next ( ind-(, ind-) ) in template starting from i. If none, return (x, -1).
	// Assumes all '(' have a closing ')'
	private Vector2 ParseParan (string template, int i) {
		int j = -1;
		while (i < template.Length) {
			if (template [i] == '(') {
				j = i;
				while (true) {
					if (template [j] == ')') {
						break;
					}
					j++;
				}
				break;
			}
			i++;
		}
		return new Vector2 (i, j);
	}

}
