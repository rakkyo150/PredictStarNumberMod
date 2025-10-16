fn main() {
    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_dll_name("Libs/PredictStarNumberModLib.dll")
        .csharp_use_nint_types(false)
        .generate_csharp_file("Star/Predict.cs")
        .unwrap();
}