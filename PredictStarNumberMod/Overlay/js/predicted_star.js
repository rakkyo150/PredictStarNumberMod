const skip_star = -1;

let predicted_star = null;
let predicted_star_visibility = null;

ex_hello.push((data) => {
	predicted_star = document.getElementById("predicted_star");
	if (predicted_star == null) {
		console.error("predicted_star not found");
		return;
	}
	predicted_star.innerText = "";

	const predicted_star_group = document.getElementById("predicted_star_group");
	if (predicted_star_group == null) return;
	predicted_star_group.style.visibility = "hidden";
});

ex_other.push((data) => {
	if (typeof data.other === "undefined") {
		hidePredictedStarVisibility();
		return;
	}

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

	predicted_star.innerText = data.other.PredictedStar.toFixed(2);
});

function hidePredictedStarVisibility() {
	if (predicted_star_visibility == null) predicted_star_visibility = document.getElementById("predicted_star_visibility");

	if (predicted_star_visibility != null) predicted_star_visibility.style.visibility = "hidden";
	else predicted_star.style.visibility == "hidden"
}

function checkPredictedStarVisible() {
	if (predicted_star.style.visibility == "hidden") predicted_star.style.visibility = "visible";

	if (predicted_star_visibility == null) predicted_star_visibility = document.getElementById("predicted_star_visibility");
	if (predicted_star_visibility == null) return;

	if (predicted_star_visibility.style.visibility == "hidden") predicted_star_visibility.style.visibility = "visible";
}