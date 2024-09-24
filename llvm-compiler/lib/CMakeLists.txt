add_executable(${PROJECT_NAME}
	
	
	main.cpp)

llvm_map_components_to_libnames(llvmLibs ${LLVM_TARGETS_TO_BUILD} Core Passes Support ExecutionEngine OrcJit McJit Interpreter)
target_link_libraries(${PROJECT_NAME} PUBLIC ${llvmLibs})
set_target_properties(${PROJECT_NAME} 
	PROPERTIES
		CXX_STANDARD 20
		CXX_STANDARD_REQUIRED true)

install(TARGETS ${PROJECT_NAME})