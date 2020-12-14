﻿namespace MapNew

open System.Collections.Generic

module Sorting =


    let inline private mergeSeq (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : ('Key * 'Value)[]) (dst : ('Key * 'Value)[]) =
        let le = ri
        let re = min src.Length (ri + len)
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable lv = src.[li]
        let mutable (lk, _) = lv
        let mutable rv = src.[ri]
        let mutable (rk, _) = rv

        while li < le && ri < re do
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                dst.[oi] <- lv
                oi <- oi + 1
                li <- li + 1
                if li < le then
                    let v = src.[li]
                    let (k, _) = v
                    lk <- k
                    lv <- v
            else
                dst.[oi] <- rv
                oi <- oi + 1
                ri <- ri + 1
                if ri < re then
                    let v = src.[ri]
                    let (k, _) = v
                    rk <- k
                    rv <- v

        while li < le do
            dst.[oi] <- src.[li]
            oi <- oi + 1
            li <- li + 1
                
        while ri < re do
            dst.[oi] <- src.[ri]
            oi <- oi + 1
            ri <- ri + 1

    let inline private mergeSeqHandleDuplicates (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : ('Key * 'Value)[]) (dst : ('Key * 'Value)[]) =
        let le = ri
        let re = min src.Length (ri + len)
        let start = li
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable lv = src.[li]
        let mutable (lk, _) = lv
        let mutable rv = src.[ri]
        let mutable (rk, _) = rv
        let mutable lastKey = Unchecked.defaultof<'Key>

        let inline append (v : ('Key * 'Value)) =
            let k,_ = v
            if oi > start && cmp.Compare(k, lastKey) = 0 then
                dst.[oi-1] <- v
                lastKey <- k
            else
                dst.[oi] <- v
                lastKey <- k
                oi <- oi + 1

        while li < le && ri < re do
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                append lv
                li <- li + 1
                if li < le then
                    let v = src.[li]
                    let (k, _) = v
                    lk <- k
                    lv <- v
            else
                append rv
                ri <- ri + 1
                if ri < re then
                    let v = src.[ri]
                    let (k, _) = v
                    rk <- k
                    rv <- v

        while li < le do
            append src.[li]
            li <- li + 1
                
        while ri < re do
            append src.[ri]
            ri <- ri + 1

        oi



    let mergeSort (mutateArray : bool) (cmp : IComparer<'Key>) (arr : ('Key * 'Value)[]) =
        if arr.Length <= 1 then 
            arr
        else
            let mutable src = Array.zeroCreate arr.Length
            let mutable dst = 
                // mutateArray => allowed to mutate arr
                if mutateArray then arr
                else Array.zeroCreate arr.Length

            // copy to sorted pairs
            let mutable i0 = 0
            let mutable i1 = 1
            while i1 < arr.Length do
                let va = arr.[i0]
                let ka,_ = va 
                let vb = arr.[i1]
                let kb,_ = vb

                let c = cmp.Compare(ka, kb)
                if c <= 0 then
                    src.[i0] <- va
                    src.[i1] <- vb
                else
                    src.[i0] <- vb
                    src.[i1] <- va
                    
                i0 <- i0 + 2
                i1 <- i1 + 2

            if i0 < arr.Length then
                src.[i0] <- arr.[i0]
                i0 <- i0 + 1

            // merge sorted parts of length `sortedLength`
            let mutable sortedLength = 2
            while sortedLength < arr.Length do
                let mutable li = 0
                let mutable ri = sortedLength

                // merge case
                while ri < arr.Length do
                    mergeSeq cmp li ri sortedLength src dst
                    li <- ri + sortedLength
                    ri <- li + sortedLength

                // right got empty
                while li < arr.Length do
                    dst.[li] <- src.[li]
                    li <- li + 1
                    
                // sortedLength * 2
                sortedLength <- sortedLength <<< 1
                
                // swap src and dst
                let t = dst
                dst <- src
                src <- t

            src

    let mergeSortHandleDuplicates (mutateArray : bool) (cmp : IComparer<'Key>) (arr : ('Key * 'Value)[]) =
        let mutable src = Array.zeroCreate arr.Length
        let mutable dst = 
            // mutateArray => allowed to mutate arr
            if mutateArray then arr
            else Array.zeroCreate arr.Length

        // copy to sorted pairs
        let mutable i0 = 0
        let mutable i1 = 1
        while i1 < arr.Length do
            let va = arr.[i0]
            let ka,_ = va 
            let vb = arr.[i1]
            let kb,_ = vb

            let c = cmp.Compare(ka, kb)
            if c <= 0 then
                src.[i0] <- va
                src.[i1] <- vb
            else
                src.[i0] <- vb
                src.[i1] <- va
                    
            i0 <- i0 + 2
            i1 <- i1 + 2

        if i0 < arr.Length then
            src.[i0] <- arr.[i0]
            i0 <- i0 + 1

        let arr = ()

        // merge sorted parts of length `sortedLength`
        let mutable sortedLength = 2
        let mutable sortedLengthDbl = 4
        while sortedLengthDbl < src.Length do
            let mutable li = 0
            let mutable ri = sortedLength

            // merge case
            while ri < src.Length do
                mergeSeq cmp li ri sortedLength src dst
                li <- ri + sortedLength
                ri <- li + sortedLength

            // right got empty
            while li < src.Length do
                dst.[li] <- src.[li]
                li <- li + 1
                    
            // sortedLength * 2
            sortedLength <- sortedLengthDbl
            sortedLengthDbl <- sortedLengthDbl <<< 1
            // swap src and dst
            let t = dst
            dst <- src
            src <- t

        // final merge-dedup run
        let cnt = mergeSeqHandleDuplicates cmp 0 sortedLength sortedLength src dst
        struct(dst, cnt)

    let inline private mergeSeqV (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : struct('Key * 'Value)[]) (dst : struct('Key * 'Value)[]) =
        let le = ri
        let re = min src.Length (ri + len)
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable struct(lk, lv) = src.[li]
        let mutable struct(rk, rv) = src.[ri]

        while li < le && ri < re do
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                dst.[oi] <- struct(lk,lv)
                oi <- oi + 1
                li <- li + 1
                if li < le then
                    let struct(k, v) = src.[li]
                    lk <- k
                    lv <- v
            else
                dst.[oi] <- struct(rk,rv)
                oi <- oi + 1
                ri <- ri + 1
                if ri < re then
                    let struct(k, v) = src.[ri]
                    rk <- k
                    rv <- v

        while li < le do
            dst.[oi] <- src.[li]
            oi <- oi + 1
            li <- li + 1
                
        while ri < re do
            dst.[oi] <- src.[ri]
            oi <- oi + 1
            ri <- ri + 1

    let inline private mergeSeqHandleDuplicatesV (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : struct('Key * 'Value)[]) (dst : struct('Key * 'Value)[]) =
        let le = ri
        let re = min src.Length (ri + len)
        let start = li
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable struct(lk, lv) = src.[li]
        let mutable struct(rk, rv) = src.[ri]
        let mutable lastKey = Unchecked.defaultof<'Key>

        let inline append k v =
            if oi > start && cmp.Compare(k, lastKey) = 0 then
                dst.[oi-1] <- struct(k, v)
                lastKey <- k
            else
                dst.[oi] <- struct(k, v)
                lastKey <- k
                oi <- oi + 1

        while li < le && ri < re do
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                append lk lv
                li <- li + 1
                if li < le then
                    let struct(k, v) = src.[li]
                    lk <- k
                    lv <- v
            else
                append rk rv
                ri <- ri + 1
                if ri < re then
                    let struct(k, v) = src.[ri]
                    rk <- k
                    rv <- v

        while li < le do
            let struct(k,v) = src.[li]
            append k v
            li <- li + 1
                
        while ri < re do
            let struct(k,v) = src.[li]
            append k v
            ri <- ri + 1

        oi

    let mergeSortV (mutateArray : bool) (cmp : IComparer<'Key>) (arr : struct('Key * 'Value)[]) =
        if arr.Length <= 1 then 
            arr
        else
            let mutable src = Array.zeroCreate arr.Length
            let mutable dst = 
                // mutateArray => allowed to mutate arr
                if mutateArray then arr
                else Array.zeroCreate arr.Length

            // copy to sorted pairs
            let mutable i0 = 0
            let mutable i1 = 1
            while i1 < arr.Length do
                let struct(ka,va) = arr.[i0]
                let struct(kb,vb) = arr.[i1]

                let c = cmp.Compare(ka, kb)
                if c <= 0 then
                    src.[i0] <- struct(ka, va)
                    src.[i1] <- struct(kb, vb)
                else
                    src.[i0] <- struct(kb, vb)
                    src.[i1] <- struct(ka, va)
                    
                i0 <- i0 + 2
                i1 <- i1 + 2

            if i0 < arr.Length then
                src.[i0] <- arr.[i0]
                i0 <- i0 + 1
                
            // merge sorted parts of length `sortedLength`
            let mutable sortedLength = 2
            while sortedLength < arr.Length do
                let mutable li = 0
                let mutable ri = sortedLength

                // merge case
                while ri < arr.Length do
                    mergeSeqV cmp li ri sortedLength src dst
                    li <- ri + sortedLength
                    ri <- li + sortedLength

                // right got empty
                while li < arr.Length do
                    dst.[li] <- src.[li]
                    li <- li + 1


                // sortedLength * 2
                sortedLength <- sortedLength <<< 1
                
                // swap src and dst
                let t = dst
                dst <- src
                src <- t

            src

    let mergeSortHandleDuplicatesV (mutateArray : bool) (cmp : IComparer<'Key>) (arr : struct('Key * 'Value)[]) =
        let mutable src = Array.zeroCreate arr.Length
        let mutable dst = 
            // mutateArray => allowed to mutate arr
            if mutateArray then arr
            else Array.zeroCreate arr.Length

        // copy to sorted pairs
        let mutable i0 = 0
        let mutable i1 = 1
        while i1 < arr.Length do
            let struct(ka,va) = arr.[i0] 
            let struct(kb,vb) = arr.[i1]

            let c = cmp.Compare(ka, kb)
            if c <= 0 then
                src.[i0] <- struct(ka, va)
                src.[i1] <- struct(kb, vb)
            else
                src.[i0] <- struct(kb, vb)
                src.[i1] <- struct(ka, va)
                    
            i0 <- i0 + 2
            i1 <- i1 + 2

        if i0 < arr.Length then
            src.[i0] <- arr.[i0]
            i0 <- i0 + 1

        let arr = ()

        // merge sorted parts of length `sortedLength`
        let mutable sortedLength = 2
        let mutable sortedLengthDbl = 4
        while sortedLengthDbl <= src.Length do
            let mutable li = 0
            let mutable ri = sortedLength

            // merge case
            while ri < src.Length do
                mergeSeqV cmp li ri sortedLength src dst
                li <- ri + sortedLength
                ri <- li + sortedLength

            // right got empty
            while li < src.Length do
                dst.[li] <- src.[li]
                li <- li + 1
                    
            // sortedLength * 2
            sortedLength <- sortedLengthDbl
            sortedLengthDbl <- sortedLengthDbl <<< 1
            // swap src and dst
            let t = dst
            dst <- src
            src <- t

        if sortedLength < src.Length then
            let cnt = mergeSeqHandleDuplicatesV cmp 0 sortedLength sortedLength src dst
            struct(dst, cnt)
        else
            let mutable i = 1
            let mutable o = 1
            let mutable struct(lastKey,_) = src.[0]
            while i < src.Length do
                let struct(k,v) = src.[i]
                if cmp.Compare(lastKey, k) = 0 then
                    src.[o-1] <- struct(k,v)
                else
                    src.[o] <- struct(k,v)
                    lastKey <- k
                    o <- o + 1
                i <- i + 1

            struct(src, o)
