#pragma once
#include <cstddef>

struct GCFrame
{
      GCFrame* prev;
      void** roots;
      int root_count;
};

extern "C" {

      extern GCFrame* gc_stack;

      void gc_push_frame(GCFrame* frame);
      void gc_pop_frame();

      void* gc_alloc(size_t size);
      void gc_collect();

}