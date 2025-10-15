fn main() {
    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_dll_name("Libs/predict_star_number_mod.dll")
        .csharp_use_nint_types(false)
        .generate_csharp_file("PredictStarNumberMod/Star/Predict.cs")
        .unwrap();
}