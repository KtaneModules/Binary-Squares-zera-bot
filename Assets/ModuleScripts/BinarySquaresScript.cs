using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using KModkit;

public class BinarySquaresScript : MonoBehaviour {

	[Header("Stuff")]
	public KMBombInfo bomb;
	public KMAudio audio;

	[Header("Public Variables")]
	public KMSelectable submitButton;
	public TextMesh wrongsLabel;
	public string[] numerals = new string[16];
	public KMSelectable[] buttons = new KMSelectable[16];
	public GameObject[] buttonObjects = new GameObject[16];
	public Material[] colors;
	

	// private variables
	bool displayingWrongs = false;
	private string[] initialState = new string[16];
	private string[] currentState = new string[16];
	private string[] displayedState = new string[16];
	private string serialNumber = "";
	
	// Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	// filters
	string[] filter1 = {
		"100", "011", "001", "100",
		"110", "100", "010", "001",
		"010", "000", "100", "010",
		"100", "111", "101", "100"};
	string[] filter2 = {
		"110", "110", "111", "011",
		"010", "001", "110", "011",
		"001", "010", "111", "100",
		"101", "101", "001", "100"};
	string[] filter3 = {
		"000", "000", "000", "111",
		"101", "101", "101", "101",
		"010", "010", "011", "100",
		"001", "101", "010", "100"};
	string[] filter4 = {
		"110", "011", "110", "011",
		"100", "111", "100", "111",
		"110", "011", "110", "011",
		"100", "111", "100", "111"};
	string[] filter5 = {
		"001", "001", "001", "001",
		"001", "011", "100", "100",
		"000", "110", "011", "010",
		"111", "110", "000", "101"};
	string[] filter6 = {
		"001", "110", "010", "100",
		"111", "101", "101", "011",
		"101", "000", "100", "001",
		"000", "010", "001", "100"};
	string[] filter7 = {
		"101", "010", "010", "110",
		"000", "010", "010", "110",
		"000", "111", "101", "110",
		"100", "111", "010", "011"};
	string[] filter8 = {
		"100", "000", "101", "101",
		"110", "001", "110", "010",
		"101", "110", "010", "001",
		"111", "100", "010", "110"};

	int countPixels(string color,int start, int end){ 
		//both start and end inclusive
		int count = 0;
		for (int i = start; i<=end; i++){
			string pixel = currentState[i];
			if(pixel==color){
				count++;
			}
		}
		return count;
	}

	string giveLogGrid(string[] grid){
		return grid.Join(", ");

		//string s = "";
		//foreach (string i in grid){
		//	s = s+i+", "
		//}
	}

	string doColorOperation(string color, string a, string b){
		if (color == "000" || color == "001"){
			return ColorOperations.OR(a,b);
		}else if(color == "011" || color == "111"){
			return ColorOperations.AND(a,b);
		}else if(color == "010" || color == "100"){
			return ColorOperations.XOR(a,b);
		}else if(color == "110" || color == "101"){
			return ColorOperations.XNOR(a,b);
		}else{
			return "000";
		}
	}

	void applyFilter(string[] filter){
		Debug.LogFormat("[Binary Squares #{0}] The filter is {1}.", moduleId.ToString(), giveLogGrid(filter));

		// create modified filter
		string[] modifiedFilter = filter.Select(item => item).ToArray();
		for (int i=0; i<16; i++){
			modifiedFilter[i]=ColorOperations.XOR(modifiedFilter[i],currentState[i]);
		}

		Debug.LogFormat("[Binary Squares #{0}] The modified filter is {1}.", moduleId.ToString(), giveLogGrid(modifiedFilter));
		
		// execute operations
		for (int i=0; i<16; i++){
			currentState[i] = doColorOperation(currentState[i],modifiedFilter[i],currentState[i]);
		}

		Debug.LogFormat("[Binary Squares #{0}] The current state after applying the filter is {1}.", moduleId.ToString(), giveLogGrid(currentState));
	}

	void GenerateSolution(){
		// generate a random configuration
		for (int i = 0; i<16; i++){
			int square = UnityEngine.Random.Range(0,7);
			string newSq = Convert.ToString(square, 2).PadLeft(3, '0');
			initialState[i]=newSq;
			currentState[i]=newSq;
			displayedState[i]=newSq;
		}
		Debug.LogFormat("[Binary Squares #{0}] The initial state is {1}.", moduleId.ToString(), giveLogGrid(initialState));

		// rules !!
		int trueRuleCount = 0;

		// rule 1: If there are more than four white pixels, apply Filter 1.
		if (countPixels("111",0,15) > 4){
			Debug.LogFormat("[Binary Squares #{0}] There are more than 4 white pixels. Applying filter 1.", moduleId.ToString());
			applyFilter(filter1);
			trueRuleCount++;
		}

		// rule 2: If the top-left pixel is either black, white, or blue, apply Filter 2.
		if (currentState[0] == "001" || currentState[0] == "000" || currentState[0] == "111"){
			Debug.LogFormat("[Binary Squares #{0}] The top-left pixel is {1}. Applying filter 2.", moduleId.ToString(),currentState[0]);
			applyFilter(filter2);
			trueRuleCount++;
		}

		// apply filter 3
		Debug.LogFormat("[Binary Squares #{0}] Applying filter 3.", moduleId.ToString());
		applyFilter(filter3);
		trueRuleCount++;

		// start checking if we've already used 3 filters
		// rule 4: If the sum of the digits in the serial number is odd, apply Filter 4.
		if (trueRuleCount < 3){
			int total = 0;
			foreach(char c in serialNumber){
				if (Char.IsDigit(c)){
					total+=Convert.ToInt32(c);
				}
			}
			total = bomb.GetSerialNumberNumbers().Sum();

			if (total%2==1){
				Debug.LogFormat("[Binary Squares #{0}] The sum of the digits in the serial number is {1}. Applying filter 4.", moduleId.ToString(), total.ToString());
				applyFilter(filter4);
				trueRuleCount++;
			}
		}

		// rule 5: If there are more green squares on the top two rows than the bottom rows, apply Filter 5.
		if (trueRuleCount < 3 && countPixels("010",0,7) > countPixels("010",8,15)){
			Debug.LogFormat("[Binary Squares #{0}] There are {1} green squares on the top two rows, and there are {2} green squares on the bottom two rows. Applying filter 5.", moduleId.ToString(),countPixels("010",0,7), countPixels("010",8,15));
			applyFilter(filter5);
			trueRuleCount++;	
		}

		// apply filter 6
		if (trueRuleCount < 3){
			Debug.LogFormat("[Binary Squares #{0}] Applying filter 6.", moduleId.ToString());
			applyFilter(filter6);
			trueRuleCount++;			
		}

		// rule 7: If the serial number has a letter from A to J, apply Filter 7.
		if (trueRuleCount < 3){
			foreach (char c in bomb.GetSerialNumberLetters()){
				if ("ABCDEFGHIJ".Contains(c)){
					Debug.LogFormat("[Binary Squares #{0}] The serial number has a letter from A to J. Applying filter 7.", moduleId.ToString());
					applyFilter(filter7);
					trueRuleCount++;
					break;
				}
			}
		}

		// apply filter 8
		if (trueRuleCount < 3){
			Debug.LogFormat("[Binary Squares #{0}] Applying filter 8.", moduleId.ToString());
			applyFilter(filter8);
			trueRuleCount++;			
		}

		UpdateDisplayedState();
	}

	void Awake() {
		moduleId = moduleIdCounter++;
		//serialNumber = KMBombInfoExtensions.GetSerialNumber(bomb);
		
		for (int i = 0; i < 16; i++) 
		{
			int i_new = i;
			KMSelectable selectableButton = buttons[i_new];
			GameObject button = buttonObjects[i_new];
			selectableButton.OnInteract += delegate (){ChangeButtonColor(button,i_new); return false;};
		}
		submitButton.OnInteract += delegate (){Submit(); return false;};
	}

	void Start () {
		serialNumber = bomb.GetSerialNumber();
		GenerateSolution();
	}

	void UpdateDisplayedState(){
		// from the current displayedState variable, update the grid.
		for (int i=0; i<16; i++){
			string displayedColor = displayedState[i];
			Material[] mats = {BitsToMaterial(displayedColor)};
			buttonObjects[i].GetComponent<MeshRenderer>().sharedMaterial = BitsToMaterial(displayedColor);
		}
	}

	Material BitsToMaterial(string bits){
		int colorValue = Convert.ToInt32(bits, 2);
		return colors[colorValue];
	}
	string MaterialToBits(Material mat){
		string[] colorsStrings = {"000", "001", "010", "011", "100", "101", "110", "111"};
		for (int i=0; i<8; i++){
			string v = colorsStrings[i];
			if (BitsToMaterial(v) == mat) return v;
		}
		return "000";
	}
	int MaterialToColorId(Material mat){
		string[] colorsStrings = {"000", "001", "010", "011", "100", "101", "110", "111"};
		for (int i=0; i<8; i++){
			if (BitsToMaterial(colorsStrings[i]) == mat) return i;
		}
		return 0;
	}

	void ChangeButtonColor(GameObject button, int bId){
		// find the id of the current color of the button
		// then make it the next id in the color list
		// and update the displayedState array

		//interaction punch
		buttons[bId].AddInteractionPunch(0.75f);

		MeshRenderer renderer = button.GetComponent<MeshRenderer>();
		int ind = MaterialToColorId(renderer.sharedMaterial);
		ind = (ind+1)%8;
		//renderer.sharedMaterial = colors[ind];
		displayedState[bId] = MaterialToBits(colors[ind]);
		UpdateDisplayedState();
	}

	IEnumerator DisplayWrongs(List<int> wrongs){
		displayingWrongs=true;
		for (int i=0; i<2; i++){ // display the wrong indexes twice
			wrongsLabel.text = "!!!";
			yield return new WaitForSeconds(1.0f);
			foreach(int wrong in wrongs){
				wrongsLabel.text = numerals[wrong];
				yield return new WaitForSeconds(1.0f);
			}
		}
		wrongsLabel.text = "";
		displayingWrongs=false;
	}

	void Submit() {
		// always punch, but sometimes do nothing
		submitButton.AddInteractionPunch();
		if (displayingWrongs || moduleSolved) return; //dont do anything if we are displaying which ones are wrong
		Debug.LogFormat("[Binary Squares #{0}] Submitted sequence is as follows: {1}", moduleId.ToString(), giveLogGrid(displayedState));

		// find wrong buttons
		List<int> wrongs = new List<int>();
		for (int i=0; i<16; i++){
			if (currentState[i] != displayedState[i]){
				wrongs.Add(i);
			}
		}

		// determine what to do
		if (wrongs.Count == 0){
			//solve the module
			Debug.LogFormat("[Binary Squares #{0}] Correct sequence given; module solved!", moduleId.ToString());
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
		}else if (wrongs.Count < 5){
			// don't strike, but show the sequence of wrong indexes
			Debug.LogFormat("[Binary Squares #{0}] There are {1} wrong answers: {2}. I won't give a strike.", moduleId.ToString(), wrongs.Count, wrongs.Select(item => item+1).ToArray().Join(", "));
			StartCoroutine(DisplayWrongs(wrongs));
		}else{
			// strike and show the sequence of wrong indexes
			Debug.LogFormat("[Binary Squares #{0}] There are {1} wrong answers: {2}. Strike!", moduleId.ToString(), wrongs.Count, wrongs.Select(item => item+1).ToArray().Join(", "));
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(DisplayWrongs(wrongs));
		}
	}


	// twitch plays

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Input the correct square colors and submit via '!input (binaries separated by commas)', '!submit' ";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command){
		command = command.ToLowerInvariant();
		var match = Regex.Match(command, @"^\s*(?:input)\s*", RegexOptions.IgnoreCase);
		if (match.Success){
			// i'm sorry this is so messy
			command = command.Replace(" ","");
			command = command.Replace("input", "");

			command = command.Replace("k","000");
			command = command.Replace("w","111");

			command = command.Replace("r","100");
			command = command.Replace("g","010");
			command = command.Replace("b","001");

			command = command.Replace("y","110");
			command = command.Replace("c","011");
			command = command.Replace("m","101");

			Debug.Log(command);

			string[] splitList = command.Split(',');
			for (int i=0; i<16; i++){ 
				//this makes sure there are 16 items and they all are valid
				int __cv = Convert.ToInt32(splitList[i], 2); 
			}

			for (int i=0; i<16; i++){
				int count = 0; // just in case anything goes wrong
				while (displayedState[i]!=splitList[i] && count < 10){
					ChangeButtonColor(buttonObjects[i],i);
					count++;
					yield return new WaitForSeconds(0.05f);
				}
			}
		}

		match = Regex.Match(command, @"^\s*(?:submit)\s*", RegexOptions.IgnoreCase);
		if (match.Success){
			Submit();
		}
	}
}
