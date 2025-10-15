use std::io::BufReader;
use tract_onnx::onnx;
use tract_onnx::prelude::{
    InferenceFact,
    Tensor,
    Datum,
    Framework,
    tvec,
    InferenceModelExt
};
use tract_onnx::tract_hir::tract_ndarray::Array2;

#[unsafe(no_mangle)]
pub extern "C" fn get_predicted_values(record_pointer: *const f64, recorde_length: usize, model_pointer: *const u8, model_length: usize ) -> f64 {
    if model_pointer.is_null() || record_pointer.is_null() { return -10.0; }
    let model_buf = unsafe { std::slice::from_raw_parts(model_pointer, model_length) };
    let record = unsafe { std::slice::from_raw_parts(record_pointer, recorde_length) };
    if model_buf.len() == 0 || record.len() == 0 {  return -10.0;  }

    let model = onnx().model_for_read(&mut BufReader::new(&model_buf[..]))
        .unwrap()
        .with_input_fact(0, InferenceFact::dt_shape(f64::datum_type(), tvec![1, 15]))
        .unwrap()
        .with_output_fact(0, InferenceFact::dt_shape(f64::datum_type(), tvec![1, 1]))
        .unwrap()
        .into_optimized()
        .unwrap()
        .into_runnable()
        .unwrap();

    // Create an input Tensor
    let data: Vec<f64> = record.to_vec();
    let shape = [1, 15];
    let input = Tensor::from(Array2::<f64>::from_shape_vec(shape, data).unwrap());

    // Run the model
    let outputs = model.run(tvec!(input.into())).unwrap();

    // Extract the output tensor
    let output_tensor = &outputs[0];

    // Extract the result values
    let result = output_tensor.to_array_view::<f64>().unwrap();
    let predicted_value = result[[0, 0]];

    predicted_value
}
