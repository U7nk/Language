# CMakeList.txt : Top-level CMake project file, do global configuration
# and include sub-projects here.
#
cmake_minimum_required (VERSION 3.23)

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("llvm-compiler")

# FIND LLVM
find_package(LLVM REQUIRED CONFIG)
message(STATUS "FOUND LLVM ${LLVM_PACKAGE_VERSION}: ${LLVM_INSTALL_PREFIX}")
list(APPEND CMAKE_MODULE_PATH "${LLVM_CMAKE_DIR}")
include(AddLLVM)

# DISABLE RTTI IF LLVM WAS BUILT WITHOUT IT
if (NOT LLVM_ENABLE_RTTI)
	if(MSVC)
		string(APPEND CMAKE_CXX_FLAGS " /GR-")
	else()
		string(APPEND CMAKE_CXX_FLAGS " -fno-rtti")
	endif()
endif()

include_directories(${LLVM_INCLUDE_DIRS} ${CMAKE_SOURCE_DIR}/include)
separate_arguments(LLVM_DEFINITIONS_LIST NATIVE_COMMAND ${LLVM_DEFINITIONS})
add_definitions(${LLVM_DEFINITIONS_LIST})
add_subdirectory(lib)
