let predicted_star = null;

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
	if (typeof data.other === "undefined") return;

	if (typeof data.other.PredictedStar === "undefined") return;

	if (predicted_star == null) return;

	predicted_star.innerText = data.other.PredictedStar.toFixed(2);
});