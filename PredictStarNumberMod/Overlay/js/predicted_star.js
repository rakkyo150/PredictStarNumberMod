const skip_star = -1;
const error_star = -10
const no_predicted_pp = -1;

let predicted_star = null;
let predicted_star_visibility = null;

let best_predicted_pp = null;
let best_predicted_pp_visibility = null;

let now_predicted_pp = null;
let now_predicted_pp_visibility = null;

ex_hello.push((data) => {
	predicted_star = document.getElementById("predicted_star");
	best_predicted_pp = document.getElementById("best_predicted_pp");
	now_predicted_pp = document.getElementById("now_predicted_pp");
	if (predicted_star != null) {
		console.log("predicted_star found");
		predicted_star.innerText = "";
	}

	if (best_predicted_pp != null)
	{
		console.log("best_predicted_pp found");
        best_predicted_pp.innerText = "";
	}

	if (now_predicted_pp != null)
	{
		console.log("now_predicted_pp found");
		best_predicted_pp.innerText = "";
	}

	const predicted_star_mod_group = document.getElementById("predicted_star_mod_group");
	if (predicted_star_mod_group != null) predicted_star_mod_group.style.visibility = "hidden";

	const now_predicted_pp_group = document.getElementById("now_predicted_pp_group");
	if (now_predicted_pp_group != null) now_predicted_pp_group.style.visibility = "hidden";
});

ex_other.push((data) => {
	if (typeof data.other === "undefined") {
		hidePredictedStarVisibility();
		hideBestPredictedPPVisibility();
		hideNowPredictedPPVisibility()
		return;
	}

	setPredictedStar(data);
	setBestPredictedPP(data);
	setNowPredictedPP(data);
});

function setPredictedStar(data) {
	if (typeof data.other.PredictedStar === "undefined") {
		hidePredictedStarVisibility();
		return;
	}

	if (predicted_star == null) return;

	if (data.other.PredictedStar == skip_star) {
		hidePredictedStarVisibility();

		return;
	}

	checkPredictedStarVisible();

	if (data.other.PredictedStar == error_star) {
		predicted_star.innerText = "?";
	}

	predicted_star.innerText = data.other.PredictedStar.toFixed(2);
}

function setBestPredictedPP(data) {
	if (typeof data.other.BestPredictedPP === "undefined") {
		hideBestPredictedPPVisibility();
		return;
	}

	if (best_predicted_pp == null) return;

	if (data.other.BestPredictedPP == no_predicted_pp) {
		hideBestPredictedPPVisibility();

		return;
	}

	checkBestPredictedPPVisible();

	best_predicted_pp.innerText = data.other.BestPredictedPP.toFixed(2);
}

function setNowPredictedPP(data) {
	if (typeof data.other.NowPredictedPP === "undefined") {
		hideNowPredictedPPVisibility();
		return;
	}

	if (now_predicted_pp == null) return;

	if (data.other.NowPredictedPP == no_predicted_pp) {
		hideNowPredictedPPVisibility();

		return;
	}

	checkNowPredictedPPVisible();

	now_predicted_pp.innerText = data.other.NowPredictedPP.toFixed(2);
}

function hidePredictedStarVisibility() {
	if (predicted_star_visibility == null) predicted_star_visibility = document.getElementById("predicted_star_visibility");

	if (predicted_star_visibility != null) predicted_star_visibility.style.visibility = "hidden";
	else predicted_star.style.visibility == "hidden"
}

function hideBestPredictedPPVisibility()
{
	if (best_predicted_pp_visibility == null) best_predicted_pp_visibility = document.getElementById("best_predicted_pp_visibility");

	if (best_predicted_pp_visibility != null) best_predicted_pp_visibility.style.visibility = "hidden";
	else best_predicted_pp.style.visibility == "hidden"
}

function hideNowPredictedPPVisibility() {
	if (now_predicted_pp_visibility == null) now_predicted_pp_visibility = document.getElementById("now_predicted_pp_visibility");

	if (best_predicted_pp_visibility != null) now_predicted_pp_visibility.style.visibility = "hidden";
	else now_predicted_pp_visibility.style.visibility == "hidden"
}

function checkPredictedStarVisible() {
	if (predicted_star.style.visibility == "hidden") predicted_star.style.visibility = "visible";

	if (predicted_star_visibility == null) predicted_star_visibility = document.getElementById("predicted_star_visibility");
	if (predicted_star_visibility == null) return;

	if (predicted_star_visibility.style.visibility == "hidden") predicted_star_visibility.style.visibility = "visible";
}

function checkBestPredictedPPVisible()
{
    if (best_predicted_pp.style.visibility == "hidden") best_predicted_pp.style.visibility = "visible";

    if (best_predicted_pp_visibility == null) best_predicted_pp_visibility = document.getElementById("best_predicted_pp_visibility");
    if (best_predicted_pp_visibility == null) return;

    if (best_predicted_pp_visibility.style.visibility == "hidden") best_predicted_pp_visibility.style.visibility = "visible";
}

function checkNowPredictedPPVisible() {
	if (now_predicted_pp.style.visibility == "hidden") now_predicted_pp.style.visibility = "visible";

	if (now_predicted_pp_visibility == null) now_predicted_pp_visibility = document.getElementById("now_predicted_pp_visibility");
	if (now_predicted_pp_visibility == null) return;

	if (now_predicted_pp_visibility.style.visibility == "hidden") now_predicted_pp_visibility.style.visibility = "visible";
}