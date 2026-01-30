cmake_minimum_required(VERSION 3.10)
project(LearnOpenGL)

set(CMAKE_CXX_STANDARD 17)

add_executable(app
    src/main.cpp
    glad.c
)

target_include_directories(app PRIVATE include)

find_package(OpenGL REQUIRED)
find_package(glfw3 REQUIRED)

target_link_libraries(app
    glfw
    OpenGL::GL
)
